using EI.SI;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Data.Models;
using Server.Utils;
using Shared;
using Shared.DTOs;
using System.Net.Sockets;

namespace Server.PacketHandlers
{
    internal class LoginHandler : IPacketHandler
    {
        public ProtocolSICmdType CommandType => ProtocolSICmdType.USER_OPTION_2;

        public async Task HandleAsync(TcpClient client, byte[] encrypted)
        {
            (byte[] aesKey, byte[] aesIv) = Program.GetClientKeys(client);

            byte[] decrypted = AesUtils.Decrypt(encrypted, aesKey, aesIv);
            LoginRequest request = Serializer.Deserialize<LoginRequest>(decrypted);

            if (!await ValidateInput(client, request))
                return;

            using AppDbContext db = new();

            User? user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            bool valid = user != null && Password.Verify(request.Password, user.PasswordHash, user.Salt);

            if (!valid)
            {
                await Program.SendPacketAsync(client, "Invalid username or password.", ProtocolSICmdType.NACK);
                return;
            }

            Program.LoggedUsers.TryAdd(client, request.Username);

            LoginResponse response = new(request.Username);
            byte[] responseData = Serializer.Serialize(response);

            await Program.SendPacketAsync(client, responseData, ProtocolSICmdType.ACK);
            Console.WriteLine($"[Server] User logged in: {request.Username}");

        }

        private static async Task<bool> ValidateInput(TcpClient client, LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                await Program.SendPacketAsync(client, "Username and password are required.", ProtocolSICmdType.NACK);
                return false;
            }

            return true;
        }
    }
}
