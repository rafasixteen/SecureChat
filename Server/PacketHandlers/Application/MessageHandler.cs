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

            User sender = db.Users.First(u => u.Username == username);
            User receiver = db.Users.First(u => u.Username == request.FriendUsername);

            db.Messages.Add(new Message
            {
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                Content = request.Message,
                SentAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();

            await Program.SendPacketAsync(client, "send-message-success", "Message sent successfully.");

            TcpClient? receiverClient = _connectionManager.GetClientByUsername(request.FriendUsername);

            // If client is online, send them a notification about the new message.
            if (receiverClient != null)
            {
                MessageResponse message = new(request.Message, DateTime.UtcNow, sender.Username);
                await Program.SendPacketAsync(receiverClient, "message-received", Serializer.Serialize(message));
            }
        }
    }
}