using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Data.Models;
using Server.Transport.Connection;
using Shared.DTOs;
using System.Net.Sockets;

namespace Server.PacketHandlers.Application
{
    public class GetConversationHandler(ConnectionManager connectionManager) : IPacketHandler
    {
        private readonly ConnectionManager _connectionManager = connectionManager;

        public async Task HandleAsync(TcpClient client, byte[] payload)
        {
            if (!_connectionManager.IsAuthenticated(client))
            {
                await Program.SendPacketAsync(client, "get-conversation-failed", "Client is not authenticated.");
                return;
            }

            GetConversationRequest request = Serializer.Deserialize<GetConversationRequest>(payload);

            AppDbContext db = new();

            string username = _connectionManager.GetUsername(client);
            string friendUsername = request.FriendUsername;

            User sender = db.Users.First(u => u.Username == username);
            User receiver = db.Users.First(u => u.Username == friendUsername);

            List<Message>? messages = db.Messages.Include(m => m.Sender)
                .Where(m =>
                     (m.SenderId == sender.Id && m.ReceiverId == receiver.Id) ||
                     (m.SenderId == receiver.Id && m.ReceiverId == sender.Id))
                .OrderBy(m => m.SentAt)
                .ToList();

            GetConversationResponse response = new(
                messages.Select(m => new MessageResponse(
                    m.Content,
                    m.SentAt,
                    m.Sender.Username
                )).ToList()
            );

            await Program.SendPacketAsync(client, "get-conversation-success", Serializer.Serialize(response));
            Console.WriteLine($"[Server] Sent conversation to '{username}'.");
        }
    }
}