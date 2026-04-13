using EI.SI;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Server.PacketHandlers
{
    internal class SecretKeyHandler : IPacketHandler
    {
        public ProtocolSICmdType CommandType => ProtocolSICmdType.SECRET_KEY;

        public Task HandleAsync(TcpClient client, byte[] encrypted)
        {
            byte[] keyAndIv = Program.Rsa.Decrypt(encrypted, RSAEncryptionPadding.Pkcs1);

            byte[] key = keyAndIv.Take(32).ToArray();
            byte[] iv = keyAndIv.Skip(32).Take(16).ToArray();

            if (!Program.ConnectedClients.TryAdd(client, (key, iv)))
            {
                Console.WriteLine($"[Server] Failed to add client keys for {client.Client.RemoteEndPoint}");
                return Task.CompletedTask;
            }

            Console.WriteLine("[Server] AES key established");

            return Task.CompletedTask;
        }
    }
}
