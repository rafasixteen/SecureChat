using Microsoft.EntityFrameworkCore;
using SecureChat.Server.Data.Models;

namespace SecureChat.Server.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer("Server=localhost;Database=SecureChatDb;Trusted_Connection=True;");
        }
    }
}