using System.Net.Sockets;

namespace Server.PacketHandlers
{
    public class ApplicationDispatcher
    {
        private readonly Dictionary<string, IPacketHandler> _handlers = new();

        public ApplicationDispatcher With(string commandType, IPacketHandler handler)
        {
            if (!_handlers.TryAdd(commandType, handler))
                throw new InvalidOperationException($"Application handler for command type '{commandType}' is already registered.");

            return this;
        }

        public async Task DispatchAsync(TcpClient client, string commandType, byte[] data)
        {
            if (!_handlers.TryGetValue(commandType, out IPacketHandler? handler))
                throw new InvalidOperationException($"No application handler registered for command type: {commandType}");

            await handler.HandleAsync(client, data);
        }
    }
}
