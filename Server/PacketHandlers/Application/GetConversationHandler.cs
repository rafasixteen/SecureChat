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

        // Serve para enviar a conversa entre o utilizador autenticado e um outro utilizador (amigo) para o cliente.
        public async Task HandleAsync(TcpClient client, byte[] payload)
        {
            // Verificar autenticação
            if (!_connectionManager.IsAuthenticated(client))
            {
                await Program.SendPacketAsync(client, "get-conversation-failed", "Client is not authenticated.");
                return;
            }

            // Deserializar o pedido para obter o username do amigo com quem o utilizador autenticado quer ver a conversa
            GetConversationRequest request = Serializer.Deserialize<GetConversationRequest>(payload);

            AppDbContext db = new();

            string username = _connectionManager.GetUsername(client);
            string friendUsername = request.FriendUsername;

            // Quem enviou e quem recebeu
            User sender = db.Users.First(u => u.Username == username);
            User receiver = db.Users.First(u => u.Username == friendUsername);

            // A lista de todas as mensagens entre os dois utilizadores, ordenada por data de envio
            List<Message>? messages = db.Messages
                .Where(m =>
                     (m.SenderId == sender.Id && m.ReceiverId == receiver.Id) ||
                     (m.SenderId == receiver.Id && m.ReceiverId == sender.Id))
                .OrderBy(m => m.SentAt)
                .ToList();

            // Criar uma resposta para enviar os conteudos ao cliente
            GetConversationResponse response = new(
                messages.Select(m => new MessageResponse(
                    m.Content,
                    m.SentAt,
                    m.SenderId == sender.Id
                )).ToList()
            );

            // Enviar o Packet
            await Program.SendPacketAsync(client, "get-conversation-success", Serializer.Serialize(response));
            Console.WriteLine($"[Server] Sent conversation to '{username}'.");
        }
    }
}