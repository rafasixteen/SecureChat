using EI.SI;
using Shared;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Server
{
    internal class Program
    {
        public const int PORT = 8080;

        private static RSA _rsa = RSA.Create(2048);

        private static Dictionary<TcpClient, (byte[] aesKey, byte[] aesIv)> _clientSessions = new();

        public static void Main(string[] args)
        {
            Console.WriteLine($"[Server] Starting on port {PORT}...");

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

            PerformHandshake(client);

            try
            {
                while (client.Connected)
                {
                    int bytesRead = await stream.ReadAsync(protocol.Buffer.AsMemory(0, protocol.Buffer.Length));

                    if (bytesRead == 0)
                        break;

                    ProtocolSICmdType cmdType = protocol.GetCmdType();

                    switch (cmdType)
                    {
                        case ProtocolSICmdType.SYM_CIPHER_DATA:
                            byte[] encryptedData = protocol.GetData();
                            (byte[] aesKey, byte[] aesIv) = _clientSessions[client];
                            byte[] decryptedData = AesUtils.Decrypt(encryptedData, aesKey, aesIv);
                            string message = Encoding.UTF8.GetString(decryptedData);
                            Console.WriteLine($"[Server] Received from {client.Client.RemoteEndPoint}: {message}");
                            break;
                        case ProtocolSICmdType.EOT:
                            client.Close();
                            stream.Close();
                            Console.WriteLine($"[Server] Client {client.Client.RemoteEndPoint} sent EOT");
                            break;
                        default:
                            Console.WriteLine($"[Server] Unhandled command type from {client.Client.RemoteEndPoint}: {cmdType}");
                            break;
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

        private static async void PerformHandshake(TcpClient client)
        {
            ProtocolSI protocol = new();
            NetworkStream stream = client.GetStream();

            // Step 1: Send rsa public key to client
            string publicKey = Convert.ToBase64String(_rsa.ExportRSAPublicKey());
            byte[] keyData = Encoding.UTF8.GetBytes(publicKey);

            byte[] keyPacket = protocol.Make(ProtocolSICmdType.PUBLIC_KEY, keyData);
            await stream.WriteAsync(keyPacket);

            Console.WriteLine($"[Server] Sent public key to {client.Client.RemoteEndPoint}");

            // 🔐 Receive AES key from client
            int bytesRead = await stream.ReadAsync(protocol.Buffer.AsMemory(0, protocol.Buffer.Length));

            if (bytesRead == 0)
                throw new Exception("Client disconnected during handshake.");

            ProtocolSICmdType cmdType = protocol.GetCmdType();

            if (cmdType != ProtocolSICmdType.SECRET_KEY)
                throw new Exception("Expected AES key from client.");

            // Get encrypted key
            byte[] encryptedKey = protocol.GetData();

            // Decrypt with RSA
            byte[] keyAndIv = _rsa.Decrypt(encryptedKey, RSAEncryptionPadding.Pkcs1);

            // Extract key + IV
            byte[] aesKey = keyAndIv.Take(32).ToArray();
            byte[] aesIv = keyAndIv.Skip(32).Take(16).ToArray();

            // Store session
            _clientSessions[client] = (aesKey, aesIv);

            Console.WriteLine($"[Server] AES key established for {client.Client.RemoteEndPoint}");
        }
    }
}