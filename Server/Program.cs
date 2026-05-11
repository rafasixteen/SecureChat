using Microsoft.Extensions.DependencyInjection;
using Server.Extentions;
using Server.Transport;

namespace Server
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            ServiceCollection services = new();

            services.AddServerInfrastructure();
            services.AddProtocolHandlers();
            services.AddApplicationHandlers();

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