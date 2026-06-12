using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Server.Extentions;
using Server.Transport;
using Server.Data;
using System.Security.Cryptography;

namespace Server
{
    internal static class Program
    {
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