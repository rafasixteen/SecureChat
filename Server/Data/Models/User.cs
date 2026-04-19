using System.ComponentModel.DataAnnotations;

namespace Server.Data.Models
{
    public class User
    {
        // Id - com GUID para ser um Global Unique Identifier, garantindo unicidade em todo o sistema
        [Key]
        public Guid Id { get; set; }

        // Username - nome de usuário único para cada utilizador
        // Utilizado para autenticação e identificação do utilizador no sistema
        public required string Username { get; set; }

        // PasswordHash - hash da passe do utilizador, armazenado de forma segura para proteger a senha original
        public required string PasswordHash { get; set; }

        // Salt - valor aleatório utilizado para fortalecer a segurança do hash da senha, dificultando ataques de força bruta
        public required string Salt { get; set; }

        // Navigation properties para mensagens enviadas e recebidas pelo utilizador
        public ICollection<Message> MessagesSent { get; set; } = new List<Message>();
        public ICollection<Message> MessagesReceived { get; set; } = new List<Message>();
    }
}