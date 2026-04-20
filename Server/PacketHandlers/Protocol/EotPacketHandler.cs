using Server.Transport.Connection;
using System.Net.Sockets;

namespace Server.PacketHandlers.Protocol
{
    public class EotPacketHandler(ConnectionManager connectionManager) : IPacketHandler
    {
        private readonly ConnectionManager _connectionManager = connectionManager;

        public Task HandleAsync(TcpClient client, byte[] payload)
        {
            _connectionManager.Disconnect(client);
            return Task.CompletedTask;
        }
    }
}
