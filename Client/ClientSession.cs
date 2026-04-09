using EI.SI;
using Shared;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Client
{
    public class ClientSession
    {
        private TcpClient _client;

        private NetworkStream _stream;

        private ProtocolSI _protocol;

        private readonly Dictionary<ProtocolSICmdType, Action<byte[]>> _handlers;

        private CancellationTokenSource _listenerCts;

        private byte[]? _aesKey;

        private byte[]? _aesIv;

        public ClientSession(string serverIp, int serverPort)
        {
            _client = new TcpClient(serverIp, serverPort);

            _stream = _client.GetStream();
            _protocol = new ProtocolSI();

            _handlers = new();
            _listenerCts = new();
        }

        public async Task PerformHandshake()
        {
            // 1. Receive public key
            string publicKey = await ReceiveServerPublicKey();

            RSA rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);

            // 2. Generate AES key + IV
            (_aesKey, _aesIv) = AesUtils.GenerateKey();

            // 3. Combine key + IV
            byte[] keyAndIv = _aesKey.Concat(_aesIv).ToArray();

            // 4. Encrypt with RSA
            byte[] encryptedKey = rsa.Encrypt(keyAndIv, RSAEncryptionPadding.Pkcs1);

            // 5. Send to server
            byte[] packet = _protocol.Make(ProtocolSICmdType.SECRET_KEY, encryptedKey);
            await _stream.WriteAsync(packet);

            Console.WriteLine("[Client] Handshake complete (AES key exchanged)");
        }

        public void SendEncryptedPacket(byte[] data, ProtocolSICmdType commandType)
        {
            if (_aesKey == null || _aesIv == null)
                throw new InvalidOperationException("AES key not established.");

            byte[] encryptedData = AesUtils.Encrypt(data, _aesKey, _aesIv);
            byte[] packet = _protocol.Make(commandType, encryptedData);

            _stream.Write(packet, 0, packet.Length);
        }

        public void On(ProtocolSICmdType cmdType, Action<byte[]> handler)
        {
            _handlers[cmdType] = handler;
        }

        public void ClearHandlers()
        {
            _handlers.Clear();
        }

        public void StartListening()
        {
            _listenerCts = new CancellationTokenSource();
            Task.Run(() => ListenLoop(_listenerCts.Token));
        }

        public void StopListening()
        {
            _listenerCts?.Cancel();
        }

        private async Task ListenLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                int bytesRead = await _stream.ReadAsync(_protocol.Buffer.AsMemory(0, _protocol.Buffer.Length), token);

                // Server disconnected
                if (bytesRead == 0)
                    break;

                ProtocolSICmdType cmdType = _protocol.GetCmdType();
                byte[] data = _protocol.GetData();

                if (_handlers.TryGetValue(cmdType, out Action<byte[]>? handler))
                    handler.Invoke(data);
            }
        }

        private async Task<string> ReceiveServerPublicKey()
        {
            int bytesRead = await _stream.ReadAsync(_protocol.Buffer.AsMemory(0, _protocol.Buffer.Length));

            if (bytesRead == 0)
                throw new Exception("Server disconnected before sending public key.");

            ProtocolSICmdType cmdType = _protocol.GetCmdType();

            if (cmdType != ProtocolSICmdType.PUBLIC_KEY)
                throw new Exception("Expected public key from server.");

            byte[] data = _protocol.GetData();
            return Encoding.UTF8.GetString(data);
        }
    }
}