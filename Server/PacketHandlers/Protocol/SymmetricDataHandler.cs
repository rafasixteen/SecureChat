using EI.SI;
using Server.PacketHandlers.Application;
using Server.Transport.Connection;
using Shared;
using Shared.DTOs;
using System.Net.Sockets;

namespace Server.PacketHandlers.Protocol
{
    public class SymmetricDataHandler(ConnectionManager connections, ApplicationDispatcher dispatcher) : IProtocolPacketHandler
    {
        public ProtocolSICmdType CommandType => ProtocolSICmdType.SYM_CIPHER_DATA;

        public async Task HandleAsync(TcpClient client, byte[] data)
        {
            (byte[] key, byte[] iv) = connections.GetAesKeys(client);
            byte[] decrypted = AesUtils.Decrypt(data, key, iv);

            Envelope env = Serializer.Deserialize<Envelope>(decrypted);
            await dispatcher.DispatchAsync(client, env.CommandType, env.Payload);
        }
    }
}
