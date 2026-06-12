using Microsoft.Extensions.DependencyInjection;
using Server.PacketHandlers.Application;
using Server.PacketHandlers.Protocol;

namespace Server.Extentions
{
    public static class PacketHandlerServiceExtensions
    {
        /// <summary>
        /// Registers all packet handlers in the assembly.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddProtocolHandlers(this IServiceCollection services)
        {
            services.AddSingleton<ProtocolDispatcher>();

            services.AddSingleton<IProtocolPacketHandler, EotPacketHandler>();
            services.AddSingleton<IProtocolPacketHandler, PublicKeyHandler>();
            services.AddSingleton<IProtocolPacketHandler, SymmetricDataHandler>();

            return services;
        }

        /// <summary>
        /// Registers all application packet handlers in the assembly.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddApplicationHandlers(this IServiceCollection services)
        {
            services.AddSingleton<ApplicationDispatcher>();

            services.AddSingleton<IApplicationPacketHandler, RegisterHandler>();
            services.AddSingleton<IApplicationPacketHandler, LoginHandler>();
            services.AddSingleton<IApplicationPacketHandler, FriendsListHandler>();
            services.AddSingleton<IApplicationPacketHandler, GetConversationHandler>();
            services.AddSingleton<IApplicationPacketHandler, MessageHandler>();

            return services;
        }
    }
}
