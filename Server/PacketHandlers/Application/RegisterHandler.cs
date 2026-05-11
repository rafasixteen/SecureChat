using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Data.Models;
using Server.Transport;
using Server.Transport.Connection;
using Server.Transport.Security;
using Shared.DTOs;
using Shared.DTOs.Shared.DTOs;
using System.Net.Sockets;

namespace Server.PacketHandlers.Application
{
    public class RegisterHandler(
        AppDbContext db,
        ConnectionManager connections,
        Logger logger,
        IPacketSender sender) : IApplicationPacketHandler
    {
        public string CommandType => "register";

        public async Task HandleAsync(TcpClient client, byte[] payload)
        {
            RegisterRequest request = Serializer.Deserialize<RegisterRequest>(payload);

            if (!await ValidateInput(client, request))
            {
                logger.Log($"Register failed (input validation) for user: {request.Username}");
                return;
            }

            bool userExists = await db.Users.AnyAsync(u => u.Username == request.Username);

            if (userExists)
            {
                logger.Log($"Register failed (username taken) for user: {request.Username}");
                await sender.SendAsync(client, "register-failed", "Username already taken.");
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

            connections.SetUsername(client, request.Username);

            await sender.SendAsync(client, "register-success", new RegisterResponse(request.Username));
            logger.Log($"User registered: {request.Username}");
        }

        private async Task<bool> ValidateInput(TcpClient client, RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                await sender.SendAsync(client, "register-failed", "Username and password are required.");
                return false;
            }

            if (request.Username.Length > 32)
            {
                await sender.SendAsync(client, "register-failed", "Username must be 32 characters or fewer.");
                return false;
            }

            if (request.Password.Length < 4)
            {
                await sender.SendAsync(client, "register-failed", "Password must be at least 4 characters.");
                return false;
            }

            return true;
        }
    }
}
