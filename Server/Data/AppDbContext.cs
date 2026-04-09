using Microsoft.EntityFrameworkCore;
using SecureChat.Server.Data.Models;

namespace SecureChat.Server.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Message> Messages { get; set; }

        public AppDbContext()
        {

        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Message>()
                 .HasOne(m => m.Sender)
                 .WithMany(u => u.MessagesSent)
                 .HasForeignKey(m => m.SenderId)
                 .OnDelete(DeleteBehavior.Restrict);

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
        }
    }
}