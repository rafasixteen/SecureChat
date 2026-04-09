using EI.SI;
using System.Net.Sockets;

namespace Server.PacketHandlers
{
    internal class EotHandler : IPacketHandler
    {
        public ProtocolSICmdType CommandType => ProtocolSICmdType.EOT;

        public Task HandleAsync(TcpClient client, byte[] data, int bytesRead)
        {
            Console.WriteLine("[Server] Received EOT command. Closing connection.");

            return Task.CompletedTask;
        }
    }
}
