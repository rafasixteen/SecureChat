using Server.Transport.Connection;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Server.PacketHandlers.Protocol
{
    public class SecretKeyHandler(ConnectionManager connectionManager, RSA rsa) : IPacketHandler
    {
        private readonly ConnectionManager _connectionManager = connectionManager;

        private readonly RSA _rsa = rsa;

        // O cliente envia a chave AES criptografada com a chave pública RSA do servidor
        public Task HandleAsync(TcpClient client, byte[] payload)
        {
            byte[] decrypted = _rsa.Decrypt(payload, RSAEncryptionPadding.Pkcs1);
            (byte[] key, byte[] iv) = SplitKeyAndIv(decrypted);

            // Associar a chave AES e IV à conexão do cliente para uso em comunicações futuras
            _connectionManager.SetAesKeys(client, key, iv);
            Console.WriteLine("[Server] AES key established");

            return Task.CompletedTask;
        }

        // O cliente envia a chave AES criptografada com a chave pública RSA do servidor, e o servidor precisa dividir os bytes resultantes em chave e IV
        private static (byte[] key, byte[] iv) SplitKeyAndIv(byte[] data)
        {
            // A chave AES é composta pelos primeiros 32 bytes, e o IV pelos próximos 16 bytes
            int keySize = 32;
            int ivSize = 16;

            byte[] key = new byte[keySize];
            byte[] iv = new byte[ivSize];

            // BlockCopy faz com que os bytes sejam copiados diretamente para os arrays de chave e IV, sem a necessidade de loops ou manipulação manual
            Buffer.BlockCopy(data, 0, key, 0, keySize);
            Buffer.BlockCopy(data, keySize, iv, 0, ivSize);

            return (key, iv);
        }
    }
}
