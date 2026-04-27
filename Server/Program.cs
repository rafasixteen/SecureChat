using EI.SI;
using Server.PacketHandlers;
using Server.PacketHandlers.Application;
using Server.PacketHandlers.Protocol;
using Server.Transport.Connection;
using Server.Transport.Security;
using Shared;
using Shared.DTOs;
using Shared.Exceptions;
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

        private static readonly ConnectionManager _connectionManager = new();

        private static readonly ProtocolDispatcher _protocolDispacter = new();

        private static readonly ApplicationDispatcher _applicationDispatcher = new();

        public static async Task Main(string[] args)
        {
            Console.WriteLine($"[Server] Starting on port {PORT}...");
            Logger.Log($"Server starting on port {PORT}.");

            _protocolDispacter.With(ProtocolSICmdType.SYM_CIPHER_DATA, new SymmetricDataHandler(_connectionManager, _applicationDispatcher));
            _protocolDispacter.With(ProtocolSICmdType.SECRET_KEY, new SecretKeyHandler(_connectionManager, Rsa));
            _protocolDispacter.With(ProtocolSICmdType.EOT, new EotPacketHandler(_connectionManager));

            _applicationDispatcher.With("register", new RegisterHandler(_connectionManager));
            _applicationDispatcher.With("login", new LoginHandler(_connectionManager));
            _applicationDispatcher.With("get-friends", new FriendsListHandler(_connectionManager));
            _applicationDispatcher.With("get-conversation", new GetConversationHandler(_connectionManager));
            _applicationDispatcher.With("send-message", new MessageHandler(_connectionManager));

            using CancellationTokenSource cts = new();

            TcpListener listener = new(IPAddress.Any, PORT);
            listener.Start();

            Console.WriteLine("[Server] Listening...");
            Logger.Log("Server listening for connections.");
            Console.WriteLine("[Server] Press ENTER to shut down.");

            _ = Task.Run(() =>
            {
                Console.ReadLine();
                Console.WriteLine("[Server] Shutdown requested...");
                Logger.Log("Shutdown requested by user.");
                cts.Cancel();
            });

            try
            {
                while (!cts.IsCancellationRequested)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync(cts.Token);
                    Logger.Log($"Client connected: {client.Client.RemoteEndPoint}");
                    _connectionManager.Connect(client);
                    _ = Task.Run(() => HandleClientAsync(client), cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[Server] Shutdown signal received.");
                Logger.Log("Shutdown signal received.");
            }
            finally
            {
                listener.Stop();
                Logger.Log("Server stopped.");
                Console.WriteLine("[Server] Server stopped.");
            }
        }

        #region Client Handling

        private static async Task HandleClientAsync(TcpClient client)
        {
            ProtocolSI protocol = new();
            NetworkStream stream = client.GetStream();

            try
            {
                Logger.Log($"Handshake started with client: {client.Client.RemoteEndPoint}");
                await Handshake.SendPublicKey(client, Rsa);
                Logger.Log($"Handshake completed with client: {client.Client.RemoteEndPoint}");
                await ReceiveLoopAsync(client, protocol, stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Server] Client error {client.Client.RemoteEndPoint}: {ex.Message}");
                Logger.LogError($"Client error {client.Client.RemoteEndPoint}", ex);
            }
            finally
            {
                Logger.Log($"Client disconnected: {client.Client.RemoteEndPoint}");
                _connectionManager.Disconnect(client);
            }
        }

        private static async Task ReceiveLoopAsync(TcpClient client, ProtocolSI protocol, NetworkStream stream)
        {
            while (client.Connected)
            {
                await stream.ReadExactlyAsync(protocol.Buffer.AsMemory(0, 3)).ConfigureAwait(false);

                int dataLength = protocol.GetDataLength();

                if (dataLength > 0)
                    await stream.ReadExactlyAsync(protocol.Buffer.AsMemory(3, dataLength)).ConfigureAwait(false);

                ProtocolSICmdType commandType = protocol.GetCmdType();
                byte[] payload = protocol.GetData();

                try
                {
                    Logger.Log($"Received command {commandType} from {client.Client.RemoteEndPoint}");
                    await _protocolDispacter.DispatchAsync(client, commandType, payload);
                }
                catch (InvalidPacketException ex)
                {
                    Console.WriteLine($"[Server] Invalid packet from {client.Client.RemoteEndPoint}: {ex.Message}");
                    Logger.LogError($"Invalid packet from {client.Client.RemoteEndPoint}", ex);
                    _connectionManager.Disconnect(client);
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Server] {commandType} error: {ex}");
                    Logger.LogError($"Error processing command {commandType} from {client.Client.RemoteEndPoint}", ex);
                    await SendPacketAsync(client, "server-failed", "Internal server error");
                }
            }
        }

        #endregion

        #region Packet Sending

        public static async Task SendPacketAsync(TcpClient client, ProtocolSICmdType commandType, byte[] payload)
        {
            await SendRawAsync(client, commandType, payload).ConfigureAwait(false);
        }

        public static async Task SendPacketAsync(TcpClient client, ProtocolSICmdType commandType, string message)
        {
            byte[] payload = Encoding.UTF8.GetBytes(message);
            await SendPacketAsync(client, commandType, payload).ConfigureAwait(false);
        }

        public static async Task SendPacketAsync(TcpClient client, string commandType, byte[] payload)
        {
            Envelope env = new(commandType, payload);
            byte[] data = Serializer.Serialize(env);

            await SendPacketAsync(client, ProtocolSICmdType.SYM_CIPHER_DATA, data).ConfigureAwait(false);
        }

        public static async Task SendPacketAsync(TcpClient client, string commandType, string message)
        {
            byte[] payload = Encoding.UTF8.GetBytes(message);
            await SendPacketAsync(client, commandType, payload).ConfigureAwait(false);
        }

        private static async Task SendRawAsync(TcpClient client, ProtocolSICmdType commandType, byte[] payload)
        {
            (byte[] aesKey, byte[] aesIv) = _connectionManager.GetAesKeys(client);

            ProtocolSI protocol = new();
            NetworkStream stream = client.GetStream();

            byte[] encrypted = AesUtils.Encrypt(payload, aesKey, aesIv);
            byte[] packet = protocol.Make(commandType, encrypted);

            await stream.WriteAsync(packet).ConfigureAwait(false);
        }

        #endregion
    }
}