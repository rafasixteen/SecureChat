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
    public class GetConversationHandler(
        ConnectionManager connections,
        AppDbContext db,
        Logger logger,
        DbEncryptionSettings dbKeys,
        IPacketSender sender) : IApplicationPacketHandler
    {
        public string CommandType => "get-conversation";

        // The hard limit is 1400 (due to ProtocolSI), but we use 512 to be
        // safe and leave room for encryption overhead and JSON serialization.
        private const int MaxPacketSize = 512;

        public async Task HandleAsync(TcpClient client, byte[] payload)
        {
            if (!connections.IsAuthenticated(client))
            {
                logger.Log($"GetConversation rejected: client not authenticated.");
                await sender.SendAsync(client, "get-conversation-failed", "Client is not authenticated.");
                return;
            }

            GetConversationRequest request = Serializer.Deserialize<GetConversationRequest>(payload);

            if (string.IsNullOrWhiteSpace(request.FriendUsername))
            {
                logger.Log($"GetConversation failed: FriendUsername is empty.");
                await sender.SendAsync(client, "get-conversation-failed", "Friend username is required.");
                return;
            }

            string username = connections.GetUsername(client);
            string friendUsername = request.FriendUsername;

            User? senderUser = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
            User? receiverUser = await db.Users.FirstOrDefaultAsync(u => u.Username == friendUsername);

            if (senderUser == null || receiverUser == null)
            {
                logger.Log($"GetConversation failed: user(s) not found. Sender: {username}, Receiver: {friendUsername}");
                await sender.SendAsync(client, "get-conversation-failed", "User not found.");
                return;
            }

            List<Message> messages = await db.Messages.Include(m => m.Sender)
                .Where(m =>
                     (m.SenderId == senderUser.Id && m.ReceiverId == receiverUser.Id) ||
                     (m.SenderId == receiverUser.Id && m.ReceiverId == senderUser.Id))
                .OrderBy(m => m.SentAt)
                .ToListAsync();



            List<MessageResponse> messageResponses = new();

            foreach (Message m in messages)
            {
                string decryptedText = m.Content;

                try
                {
                    byte[] encryptedBytes = Convert.FromBase64String(m.Content);

                    byte[] decryptedBytes = AesUtils.Decrypt(encryptedBytes, dbKeys.Key, dbKeys.Iv);

                    decryptedText = Encoding.UTF8.GetString(decryptedBytes);
                    
                    logger.Log("[Decryption Success] Decrypted Message", true);
                }
                catch (Exception)
                {
                    logger.Log("[Decryption Fail] The Message Was likely unencrypted, the message will be sent without being decrypted.", true);
                }

                messageResponses.Add(new(decryptedText, m.SentAt, m.Sender.Username));
            }

            logger.Log($"[GetConversation] Found {messages.Count} messages between {username} and {friendUsername}.", true);

            List<MessageResponse> buffer = new();
            int currentSize = 0;

            int packetIndex = 0;
            int totalSent = 0;

            for (int i = 0; i < messageResponses.Count; i++)
            {
                byte[] messageBytes = Serializer.Serialize(messageResponses[i]);
                int messageSize = messageBytes.Length;

                // If adding this message exceeds limit, send packet first
                if (currentSize + messageSize > MaxPacketSize && buffer.Count > 0)
                {
                    logger.Log($"[GetConversation] Sending packet {packetIndex} with {buffer.Count} messages (~{currentSize} bytes)", true);

                    await SendChunk(client, buffer);

                    totalSent += buffer.Count;
                    packetIndex++;

                    buffer.Clear();
                    currentSize = 0;
                }

                buffer.Add(messageResponses[i]);
                currentSize += messageSize;
            }

            // Send remaining messages
            if (buffer.Count > 0)
            {
                logger.Log($"[GetConversation] Sending final packet {packetIndex} with {buffer.Count} messages (~{currentSize} bytes)", true);

                await SendChunk(client, buffer);
                totalSent += buffer.Count;
            }

            logger.Log($"[GetConversation] Done. Sent {totalSent}/{messageResponses.Count} messages in {packetIndex + 1} packets.", true);
        }

        private async Task SendChunk(TcpClient client, List<MessageResponse> messages)
        {
            await sender.SendAsync(client, "get-conversation-chunk", new GetConversationResponse(messages));
        }
    }
}