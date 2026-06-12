using EI.SI;
using Server.Transport.Connection;
using Shared;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Server.PacketHandlers.Protocol
{
    public class PublicKeyHandler(ConnectionManager connections, RSA serverRsa, Logger logger) : IProtocolPacketHandler
    {
        public ProtocolSICmdType CommandType => ProtocolSICmdType.PUBLIC_KEY;

        public async Task HandleAsync(TcpClient client, byte[] payload)
        {
            // Read the client public key
            string clientPublicKeyString = Encoding.UTF8.GetString(payload);
            using RSA clientRsa = RSA.Create();
            clientRsa.ImportRSAPublicKey(Convert.FromBase64String(clientPublicKeyString), out _);
            logger.Log("[Public Key] Successfully read the client's public key", true);


            // Server's public key
            string serverPublicKeyString = Convert.ToBase64String(serverRsa.ExportRSAPublicKey());
            byte[] serverPublicKeyBytes = Encoding.UTF8.GetBytes(serverPublicKeyString);

            // Send the server's public key back to the client
            ProtocolSI publicKeyProtocol = new();
            byte[] pubKeyPacket = publicKeyProtocol.Make(ProtocolSICmdType.PUBLIC_KEY, serverPublicKeyBytes);
            await client.GetStream().WriteAsync(pubKeyPacket);
            logger.Log("[Public Key] Sending server's public key...", true);


            // Generate AES key and IV, encrypt them with the client's public key, and send them back
            (byte[] aesKey, byte[] aesIV) = AesUtils.GenerateKey();
            connections.SetAesKeys(client, aesKey, aesIV);

            byte[] keyAndIv = CombineKeyAndIv(aesKey, aesIV);
            byte[] encryptedKey = clientRsa.Encrypt(keyAndIv, RSAEncryptionPadding.Pkcs1);

            // Make Packet and send
            ProtocolSI protocol = new();
            byte[] packet = protocol.Make(ProtocolSICmdType.SECRET_KEY, encryptedKey);
            await client.GetStream().WriteAsync(packet);
            logger.Log("[Private Key] Sending secret key to client...");
        }

        private static byte[] CombineKeyAndIv(byte[] key, byte[] iv)
        {
            byte[] combined = new byte[key.Length + iv.Length];
            Buffer.BlockCopy(key, 0, combined, 0, key.Length);
            Buffer.BlockCopy(iv, 0, combined, key.Length, iv.Length);
            return combined;
        }
    }
}
