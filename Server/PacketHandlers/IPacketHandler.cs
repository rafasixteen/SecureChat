using EI.SI;
using System.Net.Sockets;

namespace Server.PacketHandlers
{
    internal interface IPacketHandler
    {
        ProtocolSICmdType CommandType { get; }

        Task HandleAsync(TcpClient client, byte[] encrypted);
    }
}
