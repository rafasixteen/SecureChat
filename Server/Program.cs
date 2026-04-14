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

            _protocolDispacter.With(ProtocolSICmdType.SYM_CIPHER_DATA, new SymmetricDataHandler(_connectionManager, _applicationDispatcher));
            _protocolDispacter.With(ProtocolSICmdType.SECRET_KEY, new SecretKeyHandler(_connectionManager, Rsa));

            _applicationDispatcher.With("login", new LoginHandler(_connectionManager));

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
                    _connectionManager.Connect(client);
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
                await Handshake.SendPublicKey(client, Rsa);
                await ReceiveLoopAsync(client, protocol, stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Server] Client error {client.Client.RemoteEndPoint}: {ex.Message}");
            }
            finally
            {
                _connectionManager.Disconnect(client);
            }
        }

        private static async Task ReceiveLoopAsync(TcpClient client, ProtocolSI protocol, NetworkStream stream)
        {
            while (client.Connected)
            {
                int bytesRead = await stream.ReadAsync(protocol.Buffer.AsMemory(0, protocol.Buffer.Length));

                if (bytesRead == 0)
                    break;

                ProtocolSICmdType commandType = protocol.GetCmdType();
                byte[] payload = protocol.GetData();

                try
                {
                    await _protocolDispacter.DispatchAsync(client, commandType, payload);
                }
                catch (InvalidPacketException ex)
                {
                    Console.WriteLine($"[Server] Invalid packet from {client.Client.RemoteEndPoint}: {ex.Message}");
                    _connectionManager.Disconnect(client);
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Server] {commandType} error: {ex}");
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
            using NetworkStream stream = client.GetStream();

            byte[] encrypted = AesUtils.Encrypt(payload, aesKey, aesIv);
            byte[] packet = protocol.Make(commandType, encrypted);

            await stream.WriteAsync(packet).ConfigureAwait(false);
        }

        #endregion
    }
}