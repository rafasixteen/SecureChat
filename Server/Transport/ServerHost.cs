using EI.SI;
using Server.PacketHandlers.Protocol;
using Server.Transport.Connection;
using Server.Transport.Security;
using Shared.Exceptions;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Server.Transport
{
    public sealed class ServerHost(
        ConnectionManager connections,
        ProtocolDispatcher protocolDispatcher,
        IPacketSender sender,
        Logger logger)
    {
        private const int PORT = 8080;

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            TcpListener listener = new(IPAddress.Any, PORT);

            listener.Start();

            logger.Log($"Server starting on port {PORT}...", true);
            logger.Log("Server listening...", true);
            logger.Log("Press ENTER to shut down.", true);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync(cancellationToken);

                    connections.Connect(client);

                    _ = HandleClientAsync(client, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                logger.Log("Shutdown signal received.", true);
            }
            finally
            {
                listener.Stop();
                logger.Log("Server stopped.", true);
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            ProtocolSI protocol = new();
            NetworkStream stream = client.GetStream();

            try
            {
                logger.Log($"Client connected: {client.Client.RemoteEndPoint}", true);
                await ReceiveLoopAsync(client, protocol, stream, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError($"Client error {client.Client.RemoteEndPoint}", ex, true);
            }
            finally
            {
                connections.Disconnect(client);
                client.Dispose();
            }
        }

        private async Task ReceiveLoopAsync(TcpClient client, ProtocolSI protocol, NetworkStream stream, CancellationToken cancellationToken)
        {
            while (client.Connected && !cancellationToken.IsCancellationRequested)
            {
                await stream.ReadExactlyAsync(protocol.Buffer.AsMemory(0, 3), cancellationToken).ConfigureAwait(false);

                int dataLength = protocol.GetDataLength();

                if (dataLength > 0)
                    await stream.ReadExactlyAsync(protocol.Buffer.AsMemory(3, dataLength), cancellationToken).ConfigureAwait(false);

                ProtocolSICmdType commandType = protocol.GetCmdType();
                byte[] payload = protocol.GetData();

                try
                {
                    logger.Log($"Received command {commandType} from {client.Client.RemoteEndPoint}");

                    await protocolDispatcher.DispatchAsync(client, commandType, payload);
                }
                catch (InvalidPacketException ex)
                {
                    logger.LogError($"Invalid packet from {client.Client.RemoteEndPoint}", ex, true);

                    connections.Disconnect(client);
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError($"Error processing command {commandType} from {client.Client.RemoteEndPoint}", ex, true);
                    await sender.SendAsync(client, "server-failed", "Internal server error");
                }
            }
        }
    }
}