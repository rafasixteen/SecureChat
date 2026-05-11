using EI.SI;
using Server.Transport.Connection;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Server.PacketHandlers.Protocol
{
    public class SecretKeyHandler(ConnectionManager connections, RSA rsa) : IProtocolPacketHandler
    {
        public ProtocolSICmdType CommandType => ProtocolSICmdType.SECRET_KEY;

        public Task HandleAsync(TcpClient client, byte[] payload)
        {
            byte[] decrypted = rsa.Decrypt(payload, RSAEncryptionPadding.Pkcs1);
            (byte[] key, byte[] iv) = SplitKeyAndIv(decrypted);

            connections.SetAesKeys(client, key, iv);
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
