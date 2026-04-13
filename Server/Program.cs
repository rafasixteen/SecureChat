using EI.SI;
using Server.PacketHandlers;
using Shared;
using Shared.Exceptions;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Server
{
    internal static class Program
    {
        public const int PORT = 8080;

        public static readonly RSA Rsa = RSA.Create(2048);

        private static readonly PacketHandlerFactory _packetHandlerFactory = new();

        public static readonly ConcurrentDictionary<TcpClient, (byte[] aesKey, byte[] aesIv)> ConnectedClients = new();

        public static async Task Main(string[] args)
        {
            Console.WriteLine($"[Server] Starting on port {PORT}...");

            RegisterHandlers();

            using CancellationTokenSource cts = new();

            TcpListener listener = new(IPAddress.Any, PORT);
            listener.Start();

            Console.WriteLine("[Server] Listening...");
            Console.WriteLine("[Server] Press ENTER to shut down.");

            _ = Task.Run(() =>
            {
                Console.ReadLine();
                Console.WriteLine("[Server] Shutdown requested...");
                cts.Cancel();
            });

            try
            {
                while (!cts.IsCancellationRequested)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync(cts.Token);
                    Console.WriteLine($"[Server] Client connected: {client.Client.RemoteEndPoint}");
                    _ = Task.Run(() => HandleClientAsync(client), cts.Token);
                }
            }
            finally
            {
                listener.Stop();
                Console.WriteLine("[Server] Server stopped.");
            }
        }

        #region Client Handling

        private static async Task HandleClientAsync(TcpClient client)
        {
            ProtocolSI protocol = new();
            using NetworkStream stream = client.GetStream();

            try
            {
                await SendRsaPublicKeyAsync(client, protocol, stream);
                await ReceiveLoopAsync(client, protocol, stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Server] Client error {client.Client.RemoteEndPoint}: {ex.Message}");
            }
            finally
            {
                CleanupClient(client);
            }
        }

        private static async Task ReceiveLoopAsync(TcpClient client, ProtocolSI protocol, NetworkStream stream)
        {
            while (client.Connected)
            {
                int bytesRead = await stream.ReadAsync(protocol.Buffer.AsMemory(0, protocol.Buffer.Length));

                if (bytesRead == 0)
                    break;

                ProtocolSICmdType cmdType = protocol.GetCmdType();
                IPacketHandler? handler = _packetHandlerFactory.GetHandler(cmdType);

                if (handler == null)
                {
                    Console.WriteLine($"[Server] No handler for {cmdType}");
                    continue;
                }

                try
                {
                    await handler.HandleAsync(client, protocol.GetData());
                }
                catch (InvalidPacketException ex)
                {
                    Console.WriteLine($"[Server] Invalid packet from {client.Client.RemoteEndPoint}: {ex.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Server] Handler error: {ex}");
                    await SendPacketAsync(client, "Internal server error", ProtocolSICmdType.NACK);
                }
            }
        }

        #endregion

        #region Handshake

        private static async Task SendRsaPublicKeyAsync(TcpClient client, ProtocolSI protocol, NetworkStream stream)
        {
            string publicKey = Convert.ToBase64String(Rsa.ExportRSAPublicKey());

            byte[] data = Encoding.UTF8.GetBytes(publicKey);
            byte[] packet = protocol.Make(ProtocolSICmdType.PUBLIC_KEY, data);

            await stream.WriteAsync(packet);

            Console.WriteLine($"[Server] Sent public key to {client.Client.RemoteEndPoint}");
        }

        #endregion

        #region Packet Sending

        public static async Task SendPacketAsync(TcpClient client, byte[] data, ProtocolSICmdType commandType)
        {
            var (aesKey, aesIv) = GetClientKeys(client);

            ProtocolSI protocol = new();
            NetworkStream stream = client.GetStream();

            byte[] encrypted = AesUtils.Encrypt(data, aesKey, aesIv);
            byte[] packet = protocol.Make(commandType, encrypted);

            await stream.WriteAsync(packet);
        }

        public static Task SendPacketAsync(TcpClient client, string message, ProtocolSICmdType commandType)
        {
            return SendPacketAsync(client, Encoding.UTF8.GetBytes(message), commandType);
        }

        #endregion

        #region Client Key Management

        public static (byte[] aesKey, byte[] aesIv) GetClientKeys(TcpClient client)
        {
            if (!ConnectedClients.TryGetValue(client, out var keys))
                throw new Exception("Handshake not completed.");

            return keys;
        }

        #endregion

        #region Handler Registration

        private static void RegisterHandlers()
        {
            _packetHandlerFactory.Register(new SecretKeyHandler());
            _packetHandlerFactory.Register(new RegisterHandler());
            _packetHandlerFactory.Register(new LoginHandler());
        }

        #endregion

        #region Cleanup

        private static void CleanupClient(TcpClient client)
        {
            Console.WriteLine($"[Server] Client disconnected: {client.Client.RemoteEndPoint}");

            ConnectedClients.TryRemove(client, out _);
            client.Close();
        }

        #endregion
    }
}