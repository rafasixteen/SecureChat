using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Data.Models;
using Server.Transport;
using Server.Transport.Connection;
using Shared;
using Shared.DTOs;
using System.Net.Sockets;
using System.Text;

namespace Server.PacketHandlers.Application
{
    public class MessageHandler(
        AppDbContext db,
        ConnectionManager connections,
        Logger logger,
        DbEncryptionSettings dbKeys,
        IPacketSender sender) : IApplicationPacketHandler
    {
        public string CommandType => "send-message";

        private const int MaxMessageLength = 256;

        public async Task HandleAsync(TcpClient client, byte[] payload)
        {
            if (!connections.IsAuthenticated(client))
            {
                logger.Log($"Send message failed: client not authenticated.");
                await sender.SendAsync(client, "send-message-failed", "Client is not authenticated.");
                return;
            }

            SendMessageRequest request = Serializer.Deserialize<SendMessageRequest>(payload);
            string username = connections.GetUsername(client);

            User senderUser = await db.Users.FirstAsync(u => u.Username == username);
            User receiverUser = await db.Users.FirstAsync(u => u.Username == request.FriendUsername);

            byte[] textBytes = Encoding.UTF8.GetBytes(request.Message.Length > MaxMessageLength ? request.Message.Substring(0, MaxMessageLength) : request.Message);
            byte[] encryptedBytes = AesUtils.Encrypt(textBytes, dbKeys.Key, dbKeys.Iv);

            var messageContents = Convert.ToBase64String(encryptedBytes);

            db.Messages.Add(new Message
            {
                SenderId = senderUser.Id,
                ReceiverId = receiverUser.Id,
                Content = messageContents,
                SentAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();

            logger.Log($"Message sent from {senderUser.Username} to {receiverUser.Username}");

            await sender.SendAsync(client, "send-message-success", "Message sent successfully.");

            TcpClient? receiverClient = connections.GetClientByUsername(request.FriendUsername);

            // If client is online, send them a notification about the new message.
            if (receiverClient != null)
            {
                MessageResponse message = new(request.Message, DateTime.UtcNow, senderUser.Username);
                await sender.SendAsync(receiverClient, "message-received", message);
                logger.Log($"Message delivered to online user: {receiverUser.Username}");
            }
        }
    }
}