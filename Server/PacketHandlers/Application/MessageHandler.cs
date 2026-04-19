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

        // Handler de mensagens, para guardar e para entregar as mensagens
        public async Task HandleAsync(TcpClient client, byte[] payload)
        {
            // Verifica se o cliente está autenticado
            if (!_connectionManager.IsAuthenticated(client))
            {
                await Program.SendPacketAsync(client, "send-message-failed", "Client is not authenticated.");
                return;
            }

            // Deserializar o pedido para obter o username do amigo para quem o utilizador autenticado quer enviar a mensagem e o conteúdo da mensagem
            SendMessageRequest request = Serializer.Deserialize<SendMessageRequest>(payload);
            string username = _connectionManager.GetUsername(client);

            AppDbContext db = new();

            // Obter os IDs dos utilizadores remetente e destinatário para criar uma nova mensagem na base de dados
            Guid senderId = db.Users.First(u => u.Username == username).Id;
            Guid receiverId = db.Users.First(u => u.Username == request.FriendUsername).Id;

            // Adicionar mensagem à base de dados e guardar
            db.Messages.Add(new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = request.Message,
                SentAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();


            // Enviar uma resposta de sucesso para o remetente
            await Program.SendPacketAsync(client, "send-message-success", "Message sent successfully.");

            // Verificar se o destinatário está online para enviar uma notificação em tempo real
            TcpClient? receiver = _connectionManager.GetClientByUsername(request.FriendUsername);

            // Se estiver online, enviar uma notificação de nova mensagem para o destinatário
            if (receiver != null)
            {
                MessageResponse message = new(request.Message, DateTime.UtcNow, true);
                await Program.SendPacketAsync(receiver, "message-received", Serializer.Serialize(message));
            }
        }
    }
}