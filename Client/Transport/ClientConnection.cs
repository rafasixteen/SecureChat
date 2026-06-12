using EI.SI;
using Shared;
using Shared.DTOs;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Client.Transport
{
    public delegate void PacketHandler(byte[] data);

    public sealed class ClientConnection : IAsyncDisposable
    {
        private TcpClient? _client;
        private NetworkStream? _stream;

        private readonly ProtocolSI _protocol = new();
        private readonly ConcurrentDictionary<ProtocolSICmdType, PacketHandler> _protocolHandlers = new();
        private readonly ConcurrentDictionary<string, PacketHandler> _applicationHandlers = new();
        private readonly SemaphoreSlim _sendLock = new(1, 1);

        private Task _listenerTask = Task.CompletedTask;
        private CancellationTokenSource? _listenerCts;

        public RSA? ClientRsa { get; private set; }
        public byte[]? ServerPublicKeyBytes { get; private set; }

        private byte[]? _aesKey;
        private byte[]? _aesIv;

        private readonly SemaphoreSlim _connectionLock = new(1, 1);

        public bool IsConnected { get; private set; }

        #region Connect / Disconnect

        /// <summary>
        /// Opens a TCP connection. Safe to call only once; throws if already connected.
        /// </summary>
        public async Task ConnectAsync(string server, int port)
        {
            await _connectionLock.WaitAsync().ConfigureAwait(false);

            try
            {
                if (IsConnected)
                    throw new InvalidOperationException("Already connected. Call DisconnectAsync first.");

                // Create fresh instances — ensures no state from a previous connection leaks.
                _client = new TcpClient();
                await _client.ConnectAsync(server, port).ConfigureAwait(false);

                _stream = _client.GetStream();
                IsConnected = true;
            }
            catch
            {
                // If anything fails, release the partially-constructed resources immediately.
                DisposeTransport();
                throw;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        /// <summary>
        /// Stops the listener, flushes any pending send, and closes the connection.
        /// Safe to call even if not currently connected.
        /// </summary>
        public async Task DisconnectAsync()
        {
            await _connectionLock.WaitAsync().ConfigureAwait(false);

            try
            {
                if (!IsConnected)
                    return;

                // Signal the listener loop to stop and wait for it to exit cleanly
                // before we tear down the stream it is reading from.
                _listenerCts?.Cancel();
                await _listenerTask.ConfigureAwait(false);

                DisposeTransport();
                IsConnected = false;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        /// <summary>
        /// Releases stream + client without touching the connection lock.
        /// Called both on clean disconnect and on failed connect.
        /// </summary>
        private void DisposeTransport()
        {
            _listenerCts?.Dispose();
            _listenerCts = null;
            _listenerTask = Task.CompletedTask;

            _stream?.Dispose();
            _stream = null;

            _client?.Dispose();
            _client = null;

            _aesKey = null;
            _aesIv = null;
        }

        #endregion

        #region Handshake

        /// <summary>
        /// Performs the RSA/AES handshake with the server. Must be connected first.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if not connected or if handshake fails.</exception>
        public async Task PerformHandshakeAsync()
        {
            EnsureConnected();

            // Generate a new RSA key pair for this session
            ClientRsa = RSA.Create();
            string publicKey = Convert.ToBase64String(ClientRsa.ExportRSAPublicKey());

            byte[] publicBytes = Encoding.UTF8.GetBytes(publicKey);

            // Send the public key to the server
            byte[] publicPacket = _protocol.Make(ProtocolSICmdType.PUBLIC_KEY, publicBytes);
            await _stream!.WriteAsync(publicPacket).ConfigureAwait(false);

            // Get the server's public key + AES parameters
            string serverPublicKeyString = await ReceiveServerPublicKeyAsync().ConfigureAwait(false);
            ServerPublicKeyBytes = Convert.FromBase64String(serverPublicKeyString);

            await _stream!.ReadExactlyAsync(_protocol.Buffer.AsMemory(0, 3)).ConfigureAwait(false);

            // Parse the AES key and IV from the server's response
            int dataLength = _protocol.GetDataLength();
            if (dataLength > 0)
                await _stream.ReadExactlyAsync(_protocol.Buffer.AsMemory(3, dataLength)).ConfigureAwait(false);

            if (_protocol.GetCmdType() != ProtocolSICmdType.SECRET_KEY)
                throw new InvalidOperationException($"Expected SECRET_KEY response but got {_protocol.GetCmdType()}");

            byte[] encryptedAes = _protocol.GetData();
            byte[] decryptedAes = ClientRsa.Decrypt(encryptedAes, RSAEncryptionPadding.Pkcs1);

            (_aesKey, _aesIv) = SplitKeyAndIv(decryptedAes);
        }

        /// <summary>
        /// Receives and returns the server's public key string. Throws if the response is invalid.
        /// </summary>
        /// <param name="data"> The raw data bytes from the server's response, used for error reporting if parsing fails.</param>
        /// <returns> The server's public key as a base64 string.</returns>
        private static (byte[] key, byte[] iv) SplitKeyAndIv(byte[] data)
        {
            int keySize = 32;
            int ivSize = 16;
            byte[] key = new byte[keySize];
            byte[] iv = new byte[ivSize];
            Buffer.BlockCopy(data, 0, key, 0, keySize);
            Buffer.BlockCopy(data, keySize, iv, 0, ivSize);
            return (key, iv);
        }

        /// <summary>
        /// Receives the server's public key response and extracts the key string. Throws if the response is invalid.
        /// </summary>
        /// <returns> The server's public key as a base64 string.</returns>
        /// <exception cref="IOException"> Thrown if the server's response is malformed or indicates an error.</exception>
        /// <exception cref="InvalidOperationException"> Thrown if the server's response is not a PUBLIC_KEY command.</exception>
        private async Task<string> ReceiveServerPublicKeyAsync()
        {
            int bytesRead = await _stream!.ReadAsync(_protocol.Buffer).ConfigureAwait(false);

            // Ensure we got a valid response
            if (bytesRead == 0)
                throw new IOException("Disconnected before receiving public key.");

            ProtocolSICmdType cmdType = _protocol.GetCmdType();

            // If the command is an unexpected response, throw an error
            if (cmdType != ProtocolSICmdType.PUBLIC_KEY)
                throw new InvalidOperationException($"Expected {ProtocolSICmdType.PUBLIC_KEY}, got {cmdType}");

            return Encoding.UTF8.GetString(_protocol.GetData());
        }

        #endregion

        #region Send

        /// <summary>
        /// Sends a command with optional data to the server. Data will be encrypted if the handshake is complete.
        /// </summary>
        /// <param name="commandType"> The command type to send.</param>
        /// <param name="payload"> Optional data payload to include with the command. Can be null or empty.</param>
        public async Task SendPacketAsync(ProtocolSICmdType commandType, byte[] payload)
        {
            EnsureConnected();

            (byte[] aesKey, byte[] aesIv) = GetKeys();

            byte[] encrypted = AesUtils.Encrypt(payload, aesKey, aesIv);
            byte[] packet = _protocol.Make(commandType, encrypted);

            await _sendLock.WaitAsync().ConfigureAwait(false);

            // Send the packet, then release the lock immediately after to allow other sends to proceed while we wait for the network.
            try
            {
                await _stream!.WriteAsync(packet).ConfigureAwait(false);
            }
            finally {
                _sendLock.Release();
            }
        }

        /// <summary>
        /// Gets the AES key and IV, throwing an exception if they are not available (i.e. handshake not complete).
        /// </summary>
        /// <param name="commandType"> The command type to send.</param>
        /// <param name="payload"> The raw data to send. Will be encrypted before sending.</param>
        public async Task SendPacketAsync(string commandType, byte[] payload)
        {
            byte[] signature = ClientRsa!.SignData(payload, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            Envelope env = new(commandType, payload, signature);

            byte[] data = Serializer.Serialize(env);
            await SendPacketAsync(ProtocolSICmdType.SYM_CIPHER_DATA, data).ConfigureAwait(false);
        }

        #endregion

        #region Listener

        /// <summary>
        /// Starts the listener loop that continuously reads packets from the server and raises events. Should be called after a successful handshake.
        /// </summary>
        public void StartListening()
        {
            // Make sure the client and the server are connected
            EnsureConnected();
            GetKeys();

            if (_listenerCts != null)
                return;

            _listenerCts = new CancellationTokenSource();
            _listenerTask = Task.Run(() => ListenLoopAsync(_listenerCts.Token));
        }

        /// <summary>
        /// Stops the listener loop and cancels any pending reads. Should be called before disconnecting.
        /// </summary>
        /// <param name="token"> A cancellation token to use for waiting for the listener to stop. Should have a reasonable timeout to avoid hanging indefinitely.</param>
        private async Task ListenLoopAsync(CancellationToken token)
        {
            (byte[] aesKey, byte[] aesIv) = GetKeys();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    // Read the packet header (3 bytes)
                    await _stream!.ReadExactlyAsync(_protocol.Buffer.AsMemory(0, 3), token).ConfigureAwait(false);

                    int dataLength = _protocol.GetDataLength();

                    // Read the packet data based on the length specified in the header
                    if (dataLength > 0)
                        await _stream.ReadExactlyAsync(_protocol.Buffer.AsMemory(3, dataLength), token).ConfigureAwait(false);

                    byte[] decrypted = AesUtils.Decrypt(_protocol.GetData(), aesKey, aesIv);
                    ProtocolSICmdType commandType = _protocol.GetCmdType();

                    if (commandType == ProtocolSICmdType.SYM_CIPHER_DATA)
                    {
                        Envelope env = Serializer.Deserialize<Envelope>(decrypted);

                        // Verify the signature before raising the event
                        if (env.Signature == null || ServerPublicKeyBytes == null)
                            throw new Exception("Message Rejected: Missing signature or public key.");

                        // Verify the signature using the server's public key
                        using RSA serverRSA = RSA.Create();
                        serverRSA.ImportRSAPublicKey(ServerPublicKeyBytes, out _);

                        bool isAuthentic = serverRSA.VerifyData(env.Payload, env.Signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                        // If the signature is invalid, reject the message and do not raise the event
                        if (!isAuthentic)
                            throw new Exception("Message Rejected: Signature verification failed.");

                        // Throw an error if the command type is not a valid string command
                        if (!_applicationHandlers.TryGetValue(env.CommandType, out PacketHandler? appHandler))
                            throw new Exception($"No application handler for: {env.CommandType}");

                        appHandler.Invoke(env.Payload);
                    }
                    else
                    {
                        // Throw an error if the command type is not a valid non-data command
                        if (!_protocolHandlers.TryGetValue(commandType, out PacketHandler? protocolHandler))
                            throw new Exception($"No protocol handler for: {commandType}");

                        protocolHandler.Invoke(decrypted);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown path — cancellation was requested.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Client] Listener error: {ex.Message}");
            }
        }

        #endregion

        #region Handlers

        /// <summary>
        /// Registers a handler for a specific protocol command type. Protocol handlers are used for handling commands that are part of the handshake or other internal client operations, and are not intended to be used by application code.
        /// </summary>
        /// <param name="commandType"> The protocol command type to handle.</param>
        /// <param name="handler"> The handler to invoke when a packet with the specified command type is received. The handler will receive the decrypted payload as a byte array.</param>
        /// <exception cref="InvalidOperationException"> Thrown if a handler is already registered for the specified command type.</exception>
        public void On(ProtocolSICmdType commandType, PacketHandler handler)
        {
            if (!_protocolHandlers.TryAdd(commandType, handler))
                throw new InvalidOperationException($"Protocol handler for {commandType} already exists.");
        }

        /// <summary>
        /// Registers an application-level handler for a specific command type. These handlers will be invoked for decrypted messages that have been verified as authentic.
        /// </summary>
        /// <param name="commandType"> The command type to handle. This is an application-level identifier and can be any string, but should be unique across your application.</param>
        /// <param name="handler"> The handler to invoke when a message with the specified command type is received. The handler will receive the decrypted payload as a byte array.</param>
        /// <exception cref="InvalidOperationException"> Thrown if a handler for the specified command type is already registered.</exception>
        public void On(string commandType, PacketHandler handler)
        {
            if (!_applicationHandlers.TryAdd(commandType, handler))
                throw new InvalidOperationException($"Application handler for {commandType} already exists.");
        }

        /// <summary>
        /// Removes a previously registered handler for a protocol command type.
        /// </summary>
        /// <param name="commandType"> The protocol command type whose handler should be removed.</param>
        public void RemoveHandler(ProtocolSICmdType commandType)
        {
            _protocolHandlers.TryRemove(commandType, out _);
        }

        /// <summary>
        /// Removes an application handler for the specified command type. Should be called when the handler is no longer needed to prevent memory leaks.
        /// </summary>
        /// <param name="commandType"> The command type whose handler should be removed.</param>
        public void RemoveHandler(string commandType)
        {
            _applicationHandlers.TryRemove(commandType, out _);
        }

        /// <summary>
        /// Removes all protocol and application handlers. Useful for resetting state between connections or tests.
        /// </summary>
        public void ClearHandlers()
        {
            _protocolHandlers.Clear();
            _applicationHandlers.Clear();
        }

        #endregion

        #region IAsyncDisposable

        /// <summary>
        /// Asynchronously disposes the client, ensuring the connection is closed and resources are cleaned up. Should be called when the client is no longer needed.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync().ConfigureAwait(false);

            _sendLock.Dispose();
            _connectionLock.Dispose();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Ensures that the client is currently connected to the server. Throws an exception if not.
        /// </summary>
        /// <exception cref="InvalidOperationException"> Thrown if the client is not connected.</exception>
        private void EnsureConnected()
        {
            if (!IsConnected || _stream == null)
                throw new InvalidOperationException("Not connected. Call ConnectAsync first.");
        }

        /// <summary>
        /// Gets the AES key and IV from the handshake, throwing an exception if they are not available (i.e. handshake not complete).
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"> Thrown if the AES key or IV are not available.</exception>
        private (byte[] aesKey, byte[] aesIv) GetKeys()
        {
            if (_aesKey == null || _aesIv == null)
                throw new InvalidOperationException("Handshake not completed.");

            return (_aesKey, _aesIv);
        }

        #endregion
    }
}