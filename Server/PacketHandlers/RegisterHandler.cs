using EI.SI;
using Shared;
using System.Net.Sockets;
using System.Text;

namespace Server.PacketHandlers
{
    internal class RegisterHandler : IPacketHandler
    {
        public ProtocolSICmdType CommandType => ProtocolSICmdType.USER_OPTION_1;

        public async Task HandleAsync(TcpClient client, byte[] data, int bytesRead)
        {
            if (!Program.ConnectedClients.TryGetValue(client, out var session))
            {
                throw new Exception($"Client handshake was not sucessful for: {client.Client.RemoteEndPoint}");
            }

            (byte[] aesKey, byte[] aesIv) = session;

            // Decrypt the received registration data using AES

            byte[] decryptedData = AesUtils.Decrypt(data, aesKey, aesIv);
            string registrationData = Encoding.UTF8.GetString(decryptedData);

            string[] parts = registrationData.Split(":");
            string username = parts[0];
            string password = parts[1];

            Console.WriteLine("[Server] Received register message: " + registrationData);

            // Send the ACK response back to the client
            // TODO: Implement proper authentication and registration logic

            ProtocolSI protocol = new();
            NetworkStream stream = client.GetStream();

            string ackResponse = $"Registration successful for user: {username}";
            byte[] ackData = Encoding.UTF8.GetBytes(ackResponse);

            byte[] encryptedData = AesUtils.Encrypt(ackData, aesKey, aesIv);
            byte[] packet = protocol.Make(ProtocolSICmdType.ACK, encryptedData);

            await stream.WriteAsync(packet);

            Console.WriteLine("[Server] Sent ACK response to client: " + ackResponse);
        }
    }
}
