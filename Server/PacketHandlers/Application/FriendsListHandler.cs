using Server.Data;
using Server.Transport.Connection;
using Shared.DTOs;
using System.Net.Sockets;

namespace Server.PacketHandlers.Application
{
    public class FriendsListHandler(ConnectionManager connectionManager) : IPacketHandler
    {
        private readonly ConnectionManager _connectionManager = connectionManager;

        public async Task HandleAsync(TcpClient client, byte[] payload)
        {
            if (!_connectionManager.IsAuthenticated(client))
            {
                Logger.Log($"FriendsList rejected: client not authenticated.");
                await Program.SendPacketAsync(client, "friends-list-failed", "Unauthorized");
                return;
            }

            string username = _connectionManager.GetUsername(client);

            using AppDbContext db = new();

            List<string> friends = db.Users
                .Where(user => user.Username != username)
                .Select(f => f.Username)
                .ToList();

            FriendsListResponse response = new(friends);
            byte[] data = Serializer.Serialize(response);

            await Program.SendPacketAsync(client, "friends-list-success", data);
            Logger.Log($"Sent friends list to {username}: [{string.Join(",", friends)}]");
        }
    }
}
