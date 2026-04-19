using Microsoft.EntityFrameworkCore;
using Server.Data.Models;

namespace Server.Data
{
    public class AppDbContext : DbContext
    {
        // DbSets para as entidades User e Message
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }

        // Builder (Vasio)
        public AppDbContext() { }

        // Builder (Injeção de dependências)
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar a relação entre user e message para evitar erros
            // Mensagens enviadas
            modelBuilder.Entity<Message>()
                 .HasOne(m => m.Sender)
                 .WithMany(u => u.MessagesSent)
                 .HasForeignKey(m => m.SenderId)
                 .OnDelete(DeleteBehavior.Restrict);

            // Configurar a relação entre user e message para evitar erros
            // Mensagens recebidas
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.MessagesReceived)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            //options.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
            options.UseSqlServer("Server=.\\SQLEXPRESS;Database=SecureChat;Trusted_Connection=True;TrustServerCertificate=True");
            //options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=SecureChat;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }
}