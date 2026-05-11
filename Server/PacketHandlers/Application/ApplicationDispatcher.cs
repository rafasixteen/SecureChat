using System.Net.Sockets;

namespace Server.PacketHandlers.Application
{
    public class ApplicationDispatcher
    {
        private readonly Dictionary<string, IPacketHandler> _handlers;

        public ApplicationDispatcher(IEnumerable<IApplicationPacketHandler> handlers)
        {
            _handlers = handlers.ToDictionary(h => h.CommandType, h => (IPacketHandler)h);
        }

        public async Task DispatchAsync(TcpClient client, string commandType, byte[] data)
        {
            if (!_handlers.TryGetValue(commandType, out IPacketHandler? handler))
                throw new InvalidOperationException($"No application handler registered for command type: {commandType}");

            await handler.HandleAsync(client, data);
        }
    }
}
