using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Server.Extentions;
using Server.Transport;
using Server.Data;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace Server
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the server application.
        /// </summary>
        /// <exception cref="Exception"> Thrown when required configuration values are missing. </exception>
        public static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddUserSecrets(typeof(Program).Assembly)
                .Build();

            string b64Key = config["DbEncryption:Key"] ?? throw new Exception("DB Key missing from secrets!");
            string b64Iv = config["DbEncryption:Iv"] ?? throw new Exception("DB Iv missing from secrets!");

            DbEncryptionSettings dbEncryptionKeys = new(Convert.FromBase64String(b64Key), Convert.FromBase64String(b64Iv));

            ServiceCollection services = new();


            services.AddServerInfrastructure();
            services.AddProtocolHandlers();
            services.AddApplicationHandlers();
            services.AddSingleton(dbEncryptionKeys);
            services.AddSingleton<RSA>(sp => RSA.Create());

            ServiceProvider provider = services.BuildServiceProvider();

            // Create database and apply migrations
            using (IServiceScope scope = provider.CreateScope())
            {
                AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await dbContext.Database.MigrateAsync();
            }

            using CancellationTokenSource cts = new();

            _ = Task.Run(() =>
            {
                Console.ReadLine();
                cts.Cancel();
            });

            ServerHost server = provider.GetRequiredService<ServerHost>();
            await server.RunAsync(cts.Token);
        }
    }
}