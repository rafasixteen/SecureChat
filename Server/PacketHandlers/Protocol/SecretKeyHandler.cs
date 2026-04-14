using Server.Transport.Connection;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Server.PacketHandlers.Protocol
{
    public class SecretKeyHandler(ConnectionManager connectionManager, RSA rsa) : IPacketHandler
    {
        private readonly ConnectionManager _connectionManager = connectionManager;

        private readonly RSA _rsa = rsa;

        public Task HandleAsync(TcpClient client, byte[] payload)
        {
            byte[] decrypted = _rsa.Decrypt(payload, RSAEncryptionPadding.Pkcs1);
            (byte[] key, byte[] iv) = SplitKeyAndIv(decrypted);

            _connectionManager.SetAesKeys(client, key, iv);
            Console.WriteLine("[Server] AES key established");

            return Task.CompletedTask;
        }

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
    }
}
