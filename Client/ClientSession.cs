using EI.SI;
using Shared;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Client
{
    public class ClientSession : IDisposable
    {
        private TcpClient _client;

        private NetworkStream _stream;

        private ProtocolSI _protocol;

        private byte[]? _aesKey;

        private byte[]? _aesIv;

        public ClientSession(string serverIp, int serverPort)
        {
            _client = new TcpClient(serverIp, serverPort);
            _stream = _client.GetStream();
            _protocol = new ProtocolSI();
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

        public void SendMessage(string message, ProtocolSICmdType cmdType)
        {
            if (_aesKey == null || _aesIv == null)
                throw new InvalidOperationException("AES key not established.");

            byte[] encryptedData = AesUtils.Encrypt(message, _aesKey, _aesIv);
            byte[] packet = _protocol.Make(cmdType, encryptedData);

            _stream.Write(packet, 0, packet.Length);
        }

        public void Dispose()
        {
            _stream.Close();
            _client.Close();
        }
    }
}