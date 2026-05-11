using EI.SI;
using Server.Transport.Connection;
using System.Net.Sockets;

namespace Server.PacketHandlers.Protocol
{
    public class EotPacketHandler(ConnectionManager connections) : IProtocolPacketHandler
    {
        public ProtocolSICmdType CommandType => ProtocolSICmdType.EOT;

        private readonly ConnectionManager _connectionManager = connections;

        public Task HandleAsync(TcpClient client, byte[] payload)
        {
            _connectionManager.Disconnect(client);
            return Task.CompletedTask;
        }
    }
}
