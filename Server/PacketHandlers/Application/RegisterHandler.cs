using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Data.Models;
using Server.Transport.Connection;
using Server.Transport.Security;
using Shared.DTOs;
using Shared.DTOs.Shared.DTOs;
using System.Net.Sockets;

namespace Server.PacketHandlers.Application
{
    public class RegisterHandler(ConnectionManager connectionManager) : IPacketHandler
    {
        private readonly ConnectionManager _connectionManager = connectionManager;

        public async Task HandleAsync(TcpClient client, byte[] payload)
        {
            RegisterRequest request = Serializer.Deserialize<RegisterRequest>(payload);

            if (!await ValidateInput(client, request))
                return;

            using AppDbContext db = new();

            bool userExists = await db.Users.AnyAsync(u => u.Username == request.Username);

            if (userExists)
            {
                await Program.SendPacketAsync(client, "register-failed", "Username already taken.");
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

            _connectionManager.SetUsername(client, request.Username);

            RegisterResponse response = new(request.Username);
            byte[] responseData = Serializer.Serialize(response);

            await Program.SendPacketAsync(client, "register-success", responseData);
            Console.WriteLine($"[Server] User registered: {request.Username}");
        }

        private static async Task<bool> ValidateInput(TcpClient client, RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                await Program.SendPacketAsync(client, "register-failed", "Username and password are required.");
                return false;
            }

            if (request.Username.Length > 32)
            {
                await Program.SendPacketAsync(client, "register-failed", "Username must be 32 characters or fewer.");
                return false;
            }

            if (request.Password.Length < 4)
            {
                await Program.SendPacketAsync(client, "register-failed", "Password must be at least 4 characters.");
                return false;
            }

            return true;
        }
    }
}
