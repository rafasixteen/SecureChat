using System.ComponentModel.DataAnnotations;

namespace Server.Data.Models
{
    public class Message
    {
        [Key]
        public Guid Id { get; set; }

        public required string Content { get; set; }

        public Guid SenderId { get; set; }

        public User Sender { get; set; } = null!;

        public Guid ReceiverId { get; set; }

        public User Receiver { get; set; } = null!;

        public DateTime SentAt { get; set; }
    }
}
