using EI.SI;
using System.Net.Sockets;

namespace Server.Transport
{
    public interface IPacketSender
    {
        Task SendAsync(TcpClient client, ProtocolSICmdType commandType, byte[] payload);

        Task SendAsync(TcpClient client, ProtocolSICmdType commandType, string message);

        Task SendAsync<T>(TcpClient client, string commandType, T payload);

        Task SendAsync(TcpClient client, string commandType, byte[] payload);

        Task SendAsync(TcpClient client, string commandType, string message);
    }
}
