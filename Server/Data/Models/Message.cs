using System.ComponentModel.DataAnnotations;

namespace Server.Data.Models
{
    public class Message
    {
        [Key]
        public Guid Id { get; set; }

        public required string Contents { get; set; }

        public required string Signature { get; set; }

        public Guid SenderId { get; set; }

        public required User Sender { get; set; }

        public Guid ReceiverId { get; set; }

        public required User Receiver { get; set; }

        public DateTime SentAt { get; set; }
    }
}
