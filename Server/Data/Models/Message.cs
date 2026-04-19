using System.ComponentModel.DataAnnotations;

namespace Server.Data.Models
{
    public class Message
    {
        [Key]
        public Guid Id { get; set; }

        // Content - conteúdo da mensagem, armazenado como string (Encriptada)
        public required string Content { get; set; }

        // SenderID - Chave estrangeira que referencia o utilizador que enviou a mensagem
        public Guid SenderId { get; set; }
        public User Sender { get; set; } = null!;

        // ReceiverID - Chave estrangeira que referencia o utilizador que recebeu a mensagem
        public Guid ReceiverId { get; set; }
        public User Receiver { get; set; } = null!;

        // Quando foi enviada
        public DateTime SentAt { get; set; }
    }
}
