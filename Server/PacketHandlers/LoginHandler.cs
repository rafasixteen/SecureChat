using EI.SI;
using Shared;
using System.Net.Sockets;
using System.Text;

namespace Server.PacketHandlers
{
    internal class LoginHandler : IPacketHandler
    {
        public ProtocolSICmdType CommandType => ProtocolSICmdType.USER_OPTION_2;

        public Task HandleAsync(TcpClient client, byte[] data, int bytesRead)
        {
            if (!Program.ConnectedClients.TryGetValue(client, out var session))
            {
                throw new Exception($"Client handshake was not sucessful for: {client.Client.RemoteEndPoint}");
            }

            (byte[] aesKey, byte[] aesIv) = session;

            byte[] decryptedData = AesUtils.Decrypt(data, aesKey, aesIv);
            string message = Encoding.UTF8.GetString(decryptedData);

            Console.WriteLine("[Server] Received login message: " + message);

            return Task.CompletedTask;
        }
    }
}
