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

    public sealed class ClientConnection : IDisposable
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly ProtocolSI _protocol = new();

        private readonly ConcurrentDictionary<ProtocolSICmdType, PacketHandler> _protocolHandlers = new();

        private readonly ConcurrentDictionary<string, PacketHandler> _applicationHandlers = new();

        private readonly SemaphoreSlim _sendLock = new(1, 1);

        private CancellationTokenSource? _listenerCts;

        private byte[]? _aesKey;
        private byte[]? _aesIv;

        private bool _isDisposed;

        public ClientConnection(string serverIp, int serverPort)
        {
            _client = new TcpClient(serverIp, serverPort);
            _stream = _client.GetStream();
        }

        #region Handshake

        public async Task PerformHandshakeAsync()
        {
            string publicKey = await ReceiveServerPublicKeyAsync().ConfigureAwait(false);

            using RSA rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);

            (_aesKey, _aesIv) = AesUtils.GenerateKey();

            byte[] keyAndIv = CombineKeyAndIv(_aesKey, _aesIv);
            byte[] encryptedKey = rsa.Encrypt(keyAndIv, RSAEncryptionPadding.Pkcs1);
            byte[] packet = _protocol.Make(ProtocolSICmdType.SECRET_KEY, encryptedKey);

            await _stream.WriteAsync(packet).ConfigureAwait(false);
        }

        private async Task<string> ReceiveServerPublicKeyAsync()
        {
            int bytesRead = await _stream.ReadAsync(_protocol.Buffer).ConfigureAwait(false);

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
            (byte[] aesKey, byte[] aesIv) = GetKeys();

            byte[] encrypted = AesUtils.Encrypt(payload, aesKey, aesIv);
            byte[] packet = _protocol.Make(commandType, encrypted);

            await _sendLock.WaitAsync().ConfigureAwait(false);

            try
            {
                await _stream.WriteAsync(packet).ConfigureAwait(false);
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

        #region Receive / Listener

        public void StartListening()
        {
            if (_listenerCts != null)
                return;

            _listenerCts = new CancellationTokenSource();
            _ = Task.Run(() => ListenLoopAsync(_listenerCts.Token));
        }

        public void StopListening()
        {
            _listenerCts?.Cancel();
            _listenerCts?.Dispose();
            _listenerCts = null;
        }

        private async Task ListenLoopAsync(CancellationToken token)
        {
            (byte[] aesKey, byte[] aesIv) = GetKeys();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    int bytesRead = await _stream.ReadAsync(_protocol.Buffer, token).ConfigureAwait(false);

                    if (bytesRead == 0)
                        break;

                    ProtocolSICmdType commandType = _protocol.GetCmdType();

                    byte[] payload = _protocol.GetData();
                    byte[] decrypted = AesUtils.Decrypt(payload, aesKey, aesIv);

                    if (commandType == ProtocolSICmdType.SYM_CIPHER_DATA)
                    {
                        Envelope env = Serializer.Deserialize<Envelope>(decrypted);

                        if (!_applicationHandlers.TryGetValue(env.CommandType, out PacketHandler? appHandler))
                            throw new Exception($"No application handler registered for command type: {env.CommandType}");

                        appHandler.Invoke(env.Payload);
                    }
                    else
                    {
                        if (!_protocolHandlers.TryGetValue(commandType, out PacketHandler? protocolHandler))
                            throw new Exception($"No protocol handler registered for command type: {commandType}");

                        protocolHandler.Invoke(decrypted);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown
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

        #region Helpers

        private static byte[] CombineKeyAndIv(byte[] key, byte[] iv)
        {
            byte[] result = new byte[key.Length + iv.Length];
            Buffer.BlockCopy(key, 0, result, 0, key.Length);
            Buffer.BlockCopy(iv, 0, result, key.Length, iv.Length);
            return result;
        }

        private (byte[] aesKey, byte[] aesIv) GetKeys()
        {
            if (_aesKey == null || _aesIv == null)
                throw new InvalidOperationException("Handshake not completed.");

            return (_aesKey, _aesIv);
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            if (_isDisposed) return;

            StopListening();

            _stream.Dispose();
            _client.Dispose();
            _sendLock.Dispose();

            _isDisposed = true;
        }

        #endregion
    }
}