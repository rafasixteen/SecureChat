using System.Security.Cryptography;
using System.Text;

namespace Shared
{
    public static class AesUtils
    {
        public static (byte[] key, byte[] iv) GenerateKey()
        {
            using Aes aes = Aes.Create();
            aes.GenerateKey();
            aes.GenerateIV();
            return (aes.Key, aes.IV);
        }

        public static byte[] Encrypt(string plainText, byte[] key, byte[] iv)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] input = Encoding.UTF8.GetBytes(plainText);

            return encryptor.TransformFinalBlock(input, 0, input.Length);
        }

        public static byte[] Decrypt(byte[] cipher, byte[] key, byte[] iv)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        }
    }
}