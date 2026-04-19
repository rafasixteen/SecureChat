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
            // Verifica se o cliente está autenticado
            if (!_connectionManager.IsAuthenticated(client))
            {
                await Program.SendPacketAsync(client, "friends-list-failed", "Unauthorized");
                return;
            }

            // Qual o username do cliente autenticado
            string username = _connectionManager.GetUsername(client);

            using AppDbContext db = new();

            // Ver lista de amigos (Todos os utilizadores exceto o próprio)
            List<string> friends = db.Users
                .Where(user => user.Username != username) // Excluir o próprio utilizador
                .Select(f => f.Username) // f de friend
                .ToList();

            // Criar uma resposta com a lista de amigos e enviar para o cliente
            FriendsListResponse response = new(friends);
            byte[] data = Serializer.Serialize(response);

            // Enviar o packet
            await Program.SendPacketAsync(client, "friends-list-success", data);
            Console.WriteLine($"[Server] Sent friends list: '{string.Join(",", friends)}' to {username}");
        }
    }
}
