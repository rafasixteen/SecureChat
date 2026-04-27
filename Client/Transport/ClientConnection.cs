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

        public async Task PerformHandshakeAsync()
        {
            EnsureConnected();

            string publicKey = await ReceiveServerPublicKeyAsync().ConfigureAwait(false);

            using RSA rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);

            (_aesKey, _aesIv) = AesUtils.GenerateKey();

            byte[] keyAndIv = CombineKeyAndIv(_aesKey, _aesIv);
            byte[] encryptedKey = rsa.Encrypt(keyAndIv, RSAEncryptionPadding.Pkcs1);
            byte[] packet = _protocol.Make(ProtocolSICmdType.SECRET_KEY, encryptedKey);

            await _stream!.WriteAsync(packet).ConfigureAwait(false);
        }

        private async Task<string> ReceiveServerPublicKeyAsync()
        {
            int bytesRead = await _stream!.ReadAsync(_protocol.Buffer).ConfigureAwait(false);

            if (bytesRead == 0)
                throw new IOException("Disconnected before receiving public key.");

            ProtocolSICmdType cmdType = _protocol.GetCmdType();

            if (cmdType != ProtocolSICmdType.PUBLIC_KEY)
                throw new InvalidOperationException($"Expected {ProtocolSICmdType.PUBLIC_KEY}, got {cmdType}");

            return Encoding.UTF8.GetString(_protocol.GetData());
        }

        #endregion

        #region Send

        public async Task SendPacketAsync(ProtocolSICmdType commandType, byte[] payload)
        {
            EnsureConnected();

            (byte[] aesKey, byte[] aesIv) = GetKeys();

            byte[] encrypted = AesUtils.Encrypt(payload, aesKey, aesIv);
            byte[] packet = _protocol.Make(commandType, encrypted);

            await _sendLock.WaitAsync().ConfigureAwait(false);

            try
            {
                await _stream!.WriteAsync(packet).ConfigureAwait(false);
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public async Task SendPacketAsync(string commandType, byte[] payload)
        {
            Envelope env = new(commandType, payload);
            byte[] data = Serializer.Serialize(env);
            await SendPacketAsync(ProtocolSICmdType.SYM_CIPHER_DATA, data).ConfigureAwait(false);
        }

        #endregion

        #region Listener

        public void StartListening()
        {
            EnsureConnected();
            GetKeys();

            if (_listenerCts != null)
                return;

            _listenerCts = new CancellationTokenSource();
            _listenerTask = Task.Run(() => ListenLoopAsync(_listenerCts.Token));
        }

        private async Task ListenLoopAsync(CancellationToken token)
        {
            (byte[] aesKey, byte[] aesIv) = GetKeys();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    await _stream!.ReadExactlyAsync(_protocol.Buffer.AsMemory(0, 3), token).ConfigureAwait(false);

                    int dataLength = _protocol.GetDataLength();

                    if (dataLength > 0)
                        await _stream.ReadExactlyAsync(_protocol.Buffer.AsMemory(3, dataLength), token).ConfigureAwait(false);

                    byte[] decrypted = AesUtils.Decrypt(_protocol.GetData(), aesKey, aesIv);
                    ProtocolSICmdType commandType = _protocol.GetCmdType();

                    if (commandType == ProtocolSICmdType.SYM_CIPHER_DATA)
                    {
                        Envelope env = Serializer.Deserialize<Envelope>(decrypted);

                        if (!_applicationHandlers.TryGetValue(env.CommandType, out PacketHandler? appHandler))
                            throw new Exception($"No application handler for: {env.CommandType}");

                        appHandler.Invoke(env.Payload);
                    }
                    else
                    {
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

        public void On(ProtocolSICmdType commandType, PacketHandler handler)
        {
            if (!_protocolHandlers.TryAdd(commandType, handler))
                throw new InvalidOperationException($"Protocol handler for {commandType} already exists.");
        }

        public void On(string commandType, PacketHandler handler)
        {
            if (!_applicationHandlers.TryAdd(commandType, handler))
                throw new InvalidOperationException($"Application handler for {commandType} already exists.");
        }

        public void RemoveHandler(ProtocolSICmdType commandType)
        {
            _protocolHandlers.TryRemove(commandType, out _);
        }

        public void RemoveHandler(string commandType)
        {
            _applicationHandlers.TryRemove(commandType, out _);
        }

        public void ClearHandlers()
        {
            _protocolHandlers.Clear();
            _applicationHandlers.Clear();
        }

        #endregion

        #region IAsyncDisposable

        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync().ConfigureAwait(false);

            _sendLock.Dispose();
            _connectionLock.Dispose();
        }

        #endregion

        #region Helpers

        private void EnsureConnected()
        {
            if (!IsConnected || _stream == null)
                throw new InvalidOperationException("Not connected. Call ConnectAsync first.");
        }

        private (byte[] aesKey, byte[] aesIv) GetKeys()
        {
            if (_aesKey == null || _aesIv == null)
                throw new InvalidOperationException("Handshake not completed.");

            return (_aesKey, _aesIv);
        }

        private static byte[] CombineKeyAndIv(byte[] key, byte[] iv)
        {
            byte[] result = new byte[key.Length + iv.Length];
            Buffer.BlockCopy(key, 0, result, 0, key.Length);
            Buffer.BlockCopy(iv, 0, result, key.Length, iv.Length);
            return result;
        }

        #endregion
    }
}