using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Data.Models;
using Server.Transport.Connection;
using Server.Transport.Security;
using Shared.DTOs;
using System.Net.Sockets;

namespace Server.PacketHandlers.Application
{
    public class LoginHandler(ConnectionManager connectionManager) : IPacketHandler
    {
        private readonly ConnectionManager _connectionManager = connectionManager;

        public async Task HandleAsync(TcpClient client, byte[] payload)
        {
            LoginRequest request = Serializer.Deserialize<LoginRequest>(payload);

            if (!await ValidateInput(client, request))
            {
                Logger.Log($"Login failed (input validation) for user: {request.Username}");
                return;
            }

            using AppDbContext db = new();

            User? user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            bool valid = user != null && Password.Verify(request.Password, user.PasswordHash, user.Salt);

            if (!valid)
            {
                Logger.Log($"Login failed (invalid credentials) for user: {request.Username}");
                await Program.SendPacketAsync(client, "login-failed", "Invalid username or password.");
                return;
            }

            _connectionManager.SetUsername(client, request.Username);

            LoginResponse response = new(request.Username);
            byte[] responseData = Serializer.Serialize(response);

            await Program.SendPacketAsync(client, "login-success", responseData);
            Logger.Log($"User logged in: {request.Username}");

        }

        private static async Task<bool> ValidateInput(TcpClient client, LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                await Program.SendPacketAsync(client, "login-failed", "Username and password are required.");
                return false;
            }

            return true;
        }
    }
}
