using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Data.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        public required string Username { get; set; }

        public required string PasswordHash { get; set; }

        public required string Salt { get; set; }

        [InverseProperty("Sender")]
        public virtual ICollection<Message>? MessagesSent { get; set; }

        [InverseProperty("Receiver")]
        public virtual ICollection<Message>? MessagesReceived { get; set; }
    }
}