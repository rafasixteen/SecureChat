using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Transport;
using Server.Transport.Connection;
using Shared.DTOs;
using System.Net.Sockets;

namespace Server.PacketHandlers.Application
{
    public class FriendsListHandler(
        AppDbContext db,
        ConnectionManager connections,
        Logger logger,
        IPacketSender sender) : IApplicationPacketHandler
    {
        public string CommandType => "get-friends";

        public async Task HandleAsync(TcpClient client, byte[] payload)
        {
            if (!connections.IsAuthenticated(client))
            {
                logger.Log($"FriendsList rejected: client not authenticated.", true);
                await sender.SendAsync(client, "friends-list-failed", "Unauthorized");
                return;
            }

            string username = connections.GetUsername(client);

            List<string> friends = await db.Users
                .Where(user => user.Username != username)
                .Select(f => f.Username)
                .ToListAsync();

            await sender.SendAsync(client, "friends-list-success", new FriendsListResponse(friends));
            logger.Log($"Sent friends list to {username}: [{string.Join(",", friends)}]", true);
        }
    }
}
