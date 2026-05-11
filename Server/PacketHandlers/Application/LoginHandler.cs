using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Data.Models;
using Server.Transport;
using Server.Transport.Connection;
using Server.Transport.Security;
using Shared.DTOs;
using System.Net.Sockets;

namespace Server.PacketHandlers.Application
{
    public class LoginHandler(
        AppDbContext db,
        ConnectionManager connections,
        Logger logger,
        IPacketSender sender) : IApplicationPacketHandler
    {
        public string CommandType => "login";

        private readonly ConnectionManager _connectionManager = connections;

        public async Task HandleAsync(TcpClient client, byte[] payload)
        {
            LoginRequest request = Serializer.Deserialize<LoginRequest>(payload);

            if (!await ValidateInput(client, request))
            {
                logger.Log($"Login failed (input validation) for user: {request.Username}");
                return;
            }

            User? user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            bool valid = user != null && Password.Verify(request.Password, user.PasswordHash, user.Salt);

            if (!valid)
            {
                logger.Log($"Login failed (invalid credentials) for user: {request.Username}");
                await sender.SendAsync(client, "login-failed", "Invalid username or password.");
                return;
            }

            _connectionManager.SetUsername(client, request.Username);

            await sender.SendAsync(client, "login-success", new LoginResponse(request.Username));
            logger.Log($"User logged in: {request.Username}");
        }

        private async Task<bool> ValidateInput(TcpClient client, LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                await sender.SendAsync(client, "login-failed", "Username and password are required.");
                return false;
            }

            return true;
        }
    }
}
