using EI.SI;
using Shared;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Client
{
    public sealed class ClientConnection : IDisposable
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly ProtocolSI _protocol = new();

        private readonly ConcurrentDictionary<ProtocolSICmdType, Action<byte[]>> _handlers = new();
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

            await SendRawAsync(ProtocolSICmdType.SECRET_KEY, encryptedKey).ConfigureAwait(false);

            Console.WriteLine("[Client] Handshake complete");
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

        public async Task SendPacketAsync(byte[] data, ProtocolSICmdType commandType)
        {
            (byte[] aesKey, byte[] aesIv) = GetKeys();
            byte[] encrypted = AesUtils.Encrypt(data, aesKey, aesIv);

            await SendRawAsync(commandType, encrypted).ConfigureAwait(false);
        }

        private async Task SendRawAsync(ProtocolSICmdType cmdType, byte[] payload)
        {
            byte[] packet = _protocol.Make(cmdType, payload);

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

        #endregion

        #region Receive / Listener

        public void StartListening()
        {
            if (_listenerCts != null)
                throw new InvalidOperationException("Listener already running.");

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
            try
            {
                while (!token.IsCancellationRequested)
                {
                    int bytesRead = await _stream.ReadAsync(_protocol.Buffer, token).ConfigureAwait(false);

                    if (bytesRead == 0)
                        break;

                    HandleIncomingPacket();
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

        private void HandleIncomingPacket()
        {
            ProtocolSICmdType cmdType = _protocol.GetCmdType();

            if (!_handlers.TryGetValue(cmdType, out Action<byte[]>? handler))
                return;

            (byte[] aesKey, byte[] aesIv) = GetKeys();

            byte[] encrypted = _protocol.GetData();
            byte[] decrypted = AesUtils.Decrypt(encrypted, aesKey, aesIv);

            handler.Invoke(decrypted);
        }

        #endregion

        #region Handlers

        public void On(ProtocolSICmdType cmdType, Action<byte[]> handler)
        {
            _handlers[cmdType] = handler;
        }

        public void ClearHandlers()
        {
            _handlers.Clear();
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