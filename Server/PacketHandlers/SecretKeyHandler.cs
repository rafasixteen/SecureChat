using EI.SI;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Server.PacketHandlers
{
    internal class SecretKeyHandler : IPacketHandler
    {
        public ProtocolSICmdType CommandType => ProtocolSICmdType.SECRET_KEY;

        public Task HandleAsync(TcpClient client, byte[] data, int bytesRead)
        {
            byte[] keyAndIv = Program.Rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);

            byte[] key = keyAndIv.Take(32).ToArray();
            byte[] iv = keyAndIv.Skip(32).Take(16).ToArray();

            Program.ConnectedClients.Add(client, (key, iv));

            Console.WriteLine("[Server] AES key established");

            return Task.CompletedTask;
        }
    }
}
