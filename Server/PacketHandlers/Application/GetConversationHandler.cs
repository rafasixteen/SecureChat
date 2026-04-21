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

        private const int MaxPacketSize = 1024;

        public async Task HandleAsync(TcpClient client, byte[] payload)
        {
            if (!_connectionManager.IsAuthenticated(client))
            {
                Console.WriteLine("[GetConversation] Rejected request: client not authenticated.");
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

            Console.WriteLine($"[GetConversation] Found {messages.Count} messages between users.");

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

                    SendChunk(client, buffer);

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

                SendChunk(client, buffer);
                totalSent += buffer.Count;
            }

            Console.WriteLine($"[GetConversation] Done. Sent {totalSent}/{messageResponses.Count} messages in {packetIndex + 1} packets.");
        }

        private static async void SendChunk(TcpClient client, List<MessageResponse> messages)
        {
            GetConversationResponse packet = new(messages);
            byte[] data = Serializer.Serialize(packet);

            Console.WriteLine($"[GetConversation] Packet size: {data.Length} bytes");
            await Program.SendPacketAsync(client, "get-conversation-chunk", data);
        }
    }
}