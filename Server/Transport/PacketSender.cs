using EI.SI;
using Server.Transport.Connection;
using Shared;
using Shared.DTOs;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Server.Transport
{
    public class PacketSender(ConnectionManager connectionManager, RSA serverRsa) : IPacketSender
    {
        private readonly ConnectionManager _connectionManager = connectionManager;

        /// <summary>
        /// Sends a packet to the specified client. If the client is authenticated, the payload will be encrypted using the client's public key.
        /// </summary>
        /// <param name="client"> The target client.</param>
        /// <param name="commandType"> The command type of the packet.</param>
        /// <param name="message"> The message payload to send. This will be serialized to JSON and encrypted if the client is authenticated.</param> 
        public async Task SendAsync(TcpClient client, string commandType, string message)
        {
            byte[] payload = Encoding.UTF8.GetBytes(message);
            await SendAsync(client, commandType, payload);
        }

        /// <summary>
        /// Sends a packet to the specified client. If the client is authenticated, the payload will be encrypted using the client's public key.
        /// </summary>
        /// <param name="client"> The target client.</param>
        /// <param name="commandType"> The command type of the packet.</param>
        /// <param name="payload"> The raw byte payload to send. This will be encrypted if the client is authenticated.</param>
        public async Task SendAsync(TcpClient client, string commandType, byte[] payload)
        {
            byte[] signature = serverRsa.SignData(payload, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            Envelope env = new(commandType, payload, signature);
            byte[] data = Serializer.Serialize(env);
            await SendAsync(client, ProtocolSICmdType.SYM_CIPHER_DATA, data);
        }

        /// <summary>
        /// Sends a raw packet to the specified client without any additional processing. This is used for sending unencrypted responses to unauthenticated clients (e.g. error messages).
        /// </summary>
        /// <typeparam name="T"> The type of the message payload.</typeparam>
        /// <param name="client"> The target client.</param>
        /// <param name="commandType"> The command type of the packet.</param>
        /// <param name="payload"> The message payload to send. This will be serialized to JSON but NOT encrypted, even if the client is authenticated.</param>
        public Task SendAsync<T>(TcpClient client, string commandType, T payload)
        {
            byte[] data = Serializer.Serialize(payload);
            return SendAsync(client, commandType, data);
        }

        /// <summary>
        /// Sends a raw packet to the specified client without any additional processing. This is used for sending unencrypted packets, such as during authentication.
        /// </summary>
        /// <param name="client"> The target client.</param>
        /// <param name="commandType"> The command type of the packet.</param>
        /// <param name="message"> The raw message payload to send. This will be sent as-is without encryption or signing.</param>
        public async Task SendAsync(TcpClient client, ProtocolSICmdType commandType, string message)
        {
            byte[] payload = Encoding.UTF8.GetBytes(message);
            await SendAsync(client, commandType, payload);
        }

        /// <summary>
        /// Sends a packet to the specified client with the given command type and payload. This method does not perform encryption or signing, and is intended for low-level protocol communication.
        /// </summary>
        /// <param name="client"> The target client.</param>
        /// <param name="commandType"> The protocol command type to send. This is used for low-level protocol communication and should not be used for application-level messages.</param>
        /// <param name="payload"> The raw payload to send. This should already be encrypted and/or signed as needed by the protocol.</param>
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
