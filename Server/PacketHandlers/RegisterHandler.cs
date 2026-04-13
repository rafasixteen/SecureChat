using EI.SI;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Data.Models;
using Server.Utils;
using Shared;
using Shared.DTOs;
using Shared.DTOs.Shared.DTOs;
using System.Net.Sockets;

namespace Server.PacketHandlers
{
    internal class RegisterHandler : IPacketHandler
    {
        public ProtocolSICmdType CommandType => ProtocolSICmdType.USER_OPTION_1;

        public async Task HandleAsync(TcpClient client, byte[] encrypted)
        {
            (byte[] aesKey, byte[] aesIv) = Program.GetClientKeys(client);

            byte[] decrypted = AesUtils.Decrypt(encrypted, aesKey, aesIv);
            RegisterRequest request = Serializer.Deserialize<RegisterRequest>(decrypted);

            if (!await ValidateInput(client, request))
                return;

            using AppDbContext db = new();

            bool userExists = await db.Users.AnyAsync(u => u.Username == request.Username);

            if (userExists)
            {
                await Program.SendPacketAsync(client, "Username already taken.", ProtocolSICmdType.NACK);
                return;
            }

            (string hash, string salt) = Password.Hash(request.Password);

            db.Users.Add(new User()
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = hash,
                Salt = salt
            });

            await db.SaveChangesAsync();

            Program.LoggedUsers.TryAdd(client, request.Username);

            RegisterResponse response = new(request.Username);
            byte[] responseData = Serializer.Serialize(response);

            await Program.SendPacketAsync(client, responseData, ProtocolSICmdType.ACK);
            Console.WriteLine($"[Server] User registered: {request.Username}");
        }

        private static async Task<bool> ValidateInput(TcpClient client, RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                await Program.SendPacketAsync(client, "Username and password are required.", ProtocolSICmdType.NACK);
                return false;
            }

            if (request.Username.Length > 32)
            {
                await Program.SendPacketAsync(client, "Username must be 32 characters or fewer.", ProtocolSICmdType.NACK);
                return false;
            }

            if (request.Password.Length < 4)
            {
                await Program.SendPacketAsync(client, "Password must be at least 4 characters.", ProtocolSICmdType.NACK);
                return false;
            }

            return true;
        }
    }
}
