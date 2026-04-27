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

        // The hard limit is 1400 (due to ProtocolSI), but we use 512 to be
        // safe and leave room for encryption overhead and JSON serialization.
        private const int MaxPacketSize = 512;

        public async Task HandleAsync(TcpClient client, byte[] payload)
        {
            if (!_connectionManager.IsAuthenticated(client))
            {
                Logger.Log($"GetConversation rejected: client not authenticated.");
                await Program.SendPacketAsync(client, "get-conversation-failed", "Client is not authenticated.");
                return;
            }

            GetConversationRequest request = Serializer.Deserialize<GetConversationRequest>(payload);
            
            if (string.IsNullOrWhiteSpace(request.FriendUsername))
            {
                Logger.Log($"GetConversation failed: FriendUsername is empty.");
                await Program.SendPacketAsync(client, "get-conversation-failed", "Friend username is required.");
                return;
            }

            AppDbContext db = new();

            string username = _connectionManager.GetUsername(client);
            string friendUsername = request.FriendUsername;

            User? sender = db.Users.FirstOrDefault(u => u.Username == username);
            User? receiver = db.Users.FirstOrDefault(u => u.Username == friendUsername);

            if (sender == null || receiver == null)
            {
                Logger.Log($"GetConversation failed: user(s) not found. Sender: {username}, Receiver: {friendUsername}");
                await Program.SendPacketAsync(client, "get-conversation-failed", "User not found.");
                return;
            }

            List<Message>? messages = db.Messages.Include(m => m.Sender)
                .Where(m =>
                     (m.SenderId == sender.Id && m.ReceiverId == receiver.Id) ||
                     (m.SenderId == receiver.Id && m.ReceiverId == sender.Id))
                .OrderBy(m => m.SentAt)
                .ToList();

            Logger.Log($"GetConversation: Found {messages.Count} messages between {username} and {friendUsername}.");

            List<MessageResponse> messageResponses = messages.Select(m => new MessageResponse(
                m.Content,
                m.SentAt,
                m.Sender.Username
            )).ToList();

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
                    Console.WriteLine($"[GetConversation] Sending packet {packetIndex} with {buffer.Count} messages (~{currentSize} bytes)");

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
                Console.WriteLine($"[GetConversation] Sending final packet {packetIndex} with {buffer.Count} messages (~{currentSize} bytes)");

                await SendChunk(client, buffer);
                totalSent += buffer.Count;
            }

            Console.WriteLine($"[GetConversation] Done. Sent {totalSent}/{messageResponses.Count} messages in {packetIndex + 1} packets.");
        }

        private static async Task SendChunk(TcpClient client, List<MessageResponse> messages)
        {
            GetConversationResponse packet = new(messages);
            byte[] data = Serializer.Serialize(packet);

            Console.WriteLine($"[GetConversation] Packet size: {data.Length} bytes");
            await Program.SendPacketAsync(client, "get-conversation-chunk", data);
        }
    }
}