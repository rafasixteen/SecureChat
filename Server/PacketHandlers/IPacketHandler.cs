using System.Net.Sockets;

namespace Server.PacketHandlers
{
    public interface IPacketHandler
    {
        Task HandleAsync(TcpClient client, byte[] payload);
    }
}
