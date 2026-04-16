using System.ComponentModel.DataAnnotations;

namespace Server.Data.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        public required string Username { get; set; }

        public required string PasswordHash { get; set; }

        public required string Salt { get; set; }

        public ICollection<Message> MessagesSent { get; set; } = new List<Message>();

        public ICollection<Message> MessagesReceived { get; set; } = new List<Message>();
    }
}