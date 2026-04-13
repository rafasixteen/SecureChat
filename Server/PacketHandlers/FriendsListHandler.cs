using EI.SI;
using Server.Data;
using Shared.DTOs;
using System.Net.Sockets;

namespace Server.PacketHandlers
{
    internal class FriendsListHandler : IPacketHandler
    {
        public ProtocolSICmdType CommandType => ProtocolSICmdType.USER_OPTION_3;

        public async Task HandleAsync(TcpClient client, byte[] encrypted)
        {
            if (!Program.LoggedUsers.TryGetValue(client, out string? username))
            {
                await Program.SendPacketAsync(client, "Unauthorized", ProtocolSICmdType.NACK);
                return;
            }

            using AppDbContext db = new();

            List<string> friends = db.Users
                .Where(user => user.Username != username)
                .Select(f => f.Username)
                .ToList();

            FriendsListResponse response = new(friends);
            byte[] data = Serializer.Serialize(response);

            await Program.SendPacketAsync(client, data, ProtocolSICmdType.ACK);
            Console.WriteLine($"[Server] Sent friends list: '{string.Join(",", friends)}' to {username}");
        }
    }
}
