using Server.Data;
using Server.Data.Models;
using Server.Transport.Connection;
using Shared.DTOs;
using System.Net.Sockets;

namespace Server.PacketHandlers.Application
{
    public class MessageHandler(ConnectionManager connectionManager) : IPacketHandler
    {
        private readonly ConnectionManager _connectionManager = connectionManager;

        public async Task HandleAsync(TcpClient client, byte[] payload)
        {
            if (!_connectionManager.IsAuthenticated(client))
            {
                await Program.SendPacketAsync(client, "send-message-failed", "Client is not authenticated.");
                return;
            }

            SendMessageRequest request = Serializer.Deserialize<SendMessageRequest>(payload);
            string username = _connectionManager.GetUsername(client);

            AppDbContext db = new();

            Guid senderId = db.Users.First(u => u.Username == username).Id;
            Guid receiverId = db.Users.First(u => u.Username == request.FriendUsername).Id;

            db.Messages.Add(new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = request.Message,
                SentAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();

            await Program.SendPacketAsync(client, "send-message-success", "Message sent successfully.");

            TcpClient? receiver = _connectionManager.GetClientByUsername(request.FriendUsername);

            // If client is online, send them a notification about the new message.
            if (receiver != null)
            {
                MessageResponse message = new(request.Message, DateTime.UtcNow, true);
                await Program.SendPacketAsync(receiver, "message-received", Serializer.Serialize(message));
            }
        }
    }
}