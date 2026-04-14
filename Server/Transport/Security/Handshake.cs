using EI.SI;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Server.Transport.Security
{
    public static class Handshake
    {
        private readonly static ProtocolSI _protocol = new();

        public static async Task SendPublicKey(TcpClient client, RSA rsa)
        {
            string publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());

            byte[] data = Encoding.UTF8.GetBytes(publicKey);
            byte[] packet = _protocol.Make(ProtocolSICmdType.PUBLIC_KEY, data);

            NetworkStream stream = client.GetStream();
            await stream.WriteAsync(packet);

            Console.WriteLine($"[Server] Sent public key to {client.Client.RemoteEndPoint}");
        }
    }
}
