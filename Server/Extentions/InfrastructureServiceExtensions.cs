using Microsoft.Extensions.DependencyInjection;
using Server.Data;
using Server.Transport;
using Server.Transport.Connection;
using System.Security.Cryptography;

namespace Server.Extentions
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddServerInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<ServerHost>();
            services.AddSingleton<ConnectionManager>();
            services.AddSingleton<Logger>();

            services.AddSingleton<IPacketSender, PacketSender>();
            services.AddSingleton(_ => RSA.Create(2048));

            services.AddDbContext<AppDbContext>();

            return services;
        }
    }
}
