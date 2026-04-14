using Server.Transport.Connection;
using Shared;
using Shared.DTOs;
using System.Net.Sockets;

namespace Server.PacketHandlers.Protocol
{
    public class SymmetricDataHandler(ConnectionManager connectionManager, ApplicationDispatcher dispatcher) : IPacketHandler
    {
        private readonly ConnectionManager _connectionManager = connectionManager;

        private readonly ApplicationDispatcher _dispatcher = dispatcher;

        public async Task HandleAsync(TcpClient client, byte[] data)
        {
            (byte[] key, byte[] iv) = _connectionManager.GetAesKeys(client);
            byte[] decrypted = AesUtils.Decrypt(data, key, iv);

            Envelope env = Serializer.Deserialize<Envelope>(decrypted);
            await _dispatcher.DispatchAsync(client, env.CommandType, env.Payload);
        }
    }
}
