using EI.SI;
using Server.PacketHandlers;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Server
{
    internal static class Program
    {
        public const int PORT = 8080;

        public static Dictionary<TcpClient, (byte[] aesKey, byte[] aesIv)> ConnectedClients = new();

        private static readonly PacketHandlerFactory _packetHandlerFactory = new();

        public static readonly RSA Rsa = RSA.Create(2048);

        public static void Main(string[] args)
        {
            Console.WriteLine($"[Server] Starting on port {PORT}...");

            _packetHandlerFactory.Register(new EotHandler());
            _packetHandlerFactory.Register(new SecretKeyHandler());
            _packetHandlerFactory.Register(new RegisterHandler());
            _packetHandlerFactory.Register(new LoginHandler());

            TcpListener listener = new(IPAddress.Any, PORT);
            listener.Start();

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();

                Console.WriteLine($"[Server] Client connected: {client.Client.RemoteEndPoint}");

                Thread thread = new(() => HandleClient(client));
                thread.Start();
            }
        }

        private static async void HandleClient(TcpClient client)
        {
            ProtocolSI protocol = new();
            NetworkStream stream = client.GetStream();

            SendRsaPublicKey(client, protocol, stream);

            try
            {
                while (client.Connected)
                {
                    int bytesRead = await stream.ReadAsync(protocol.Buffer.AsMemory(0, protocol.Buffer.Length));

                    if (bytesRead == 0)
                        break;

                    ProtocolSICmdType cmdType = protocol.GetCmdType();
                    IPacketHandler? handler = _packetHandlerFactory.GetHandler(cmdType);

                    if (handler != null)
                    {
                        await handler.HandleAsync(client, protocol.GetData(), bytesRead);
                    }
                    else
                    {
                        Console.WriteLine($"[Server] No handler registered for command {cmdType} from {client.Client.RemoteEndPoint}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Server] Error with client {client.Client.RemoteEndPoint}: {ex.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine($"[Server] Client disconnected: {client.Client.RemoteEndPoint}");
            }
        }

        private static void SendRsaPublicKey(TcpClient client, ProtocolSI protocol, NetworkStream stream)
        {
            string publicKey = Convert.ToBase64String(Rsa.ExportRSAPublicKey());

            byte[] keyData = Encoding.UTF8.GetBytes(publicKey);
            byte[] keyPacket = protocol.Make(ProtocolSICmdType.PUBLIC_KEY, keyData);

            stream.Write(keyPacket, 0, keyPacket.Length);

            Console.WriteLine($"[Server] Sent public key to {client.Client.RemoteEndPoint}");
        }
    }
}