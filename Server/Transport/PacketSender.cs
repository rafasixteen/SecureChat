using EI.SI;
using Server.Transport.Connection;
using Shared;
using Shared.DTOs;
using System.Net.Sockets;
using System.Text;

namespace Server.Transport
{
    public class PacketSender(ConnectionManager connectionManager) : IPacketSender
    {
        private readonly ConnectionManager _connectionManager = connectionManager;

        public async Task SendAsync(TcpClient client, string commandType, string message)
        {
            byte[] payload = Encoding.UTF8.GetBytes(message);
            await SendAsync(client, commandType, payload);
        }

        public async Task SendAsync(TcpClient client, string commandType, byte[] payload)
        {
            Envelope env = new(commandType, payload);
            byte[] data = Serializer.Serialize(env);
            await SendAsync(client, ProtocolSICmdType.SYM_CIPHER_DATA, data);
        }

        public Task SendAsync<T>(TcpClient client, string commandType, T payload)
        {
            byte[] data = Serializer.Serialize(payload);
            return SendAsync(client, commandType, data);
        }
        public async Task SendAsync(TcpClient client, ProtocolSICmdType commandType, string message)
        {
            byte[] payload = Encoding.UTF8.GetBytes(message);
            await SendAsync(client, commandType, payload);
        }

        public async Task SendAsync(TcpClient client, ProtocolSICmdType commandType, byte[] payload)
        {
            (byte[] aesKey, byte[] aesIv) = _connectionManager.GetAesKeys(client);

            ProtocolSI protocol = new();
            NetworkStream stream = client.GetStream();

            byte[] encrypted = AesUtils.Encrypt(payload, aesKey, aesIv);
            byte[] packet = protocol.Make(commandType, encrypted);

            await stream.WriteAsync(packet);
        }

    }
}
