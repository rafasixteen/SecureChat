using EI.SI;
using System.Net.Sockets;

namespace Server.PacketHandlers
{
    public class ProtocolDispatcher
    {
        private readonly Dictionary<ProtocolSICmdType, IPacketHandler> _handlers = new();

        public ProtocolDispatcher With(ProtocolSICmdType commandType, IPacketHandler handler)
        {
            if (!_handlers.TryAdd(commandType, handler))
                throw new InvalidOperationException($"Protocol handler for command type '{commandType}' is already registered.");

            return this;
        }

        public async Task DispatchAsync(TcpClient client, ProtocolSICmdType commandType, byte[] data)
        {
            if (!_handlers.TryGetValue(commandType, out IPacketHandler? handler))
                throw new InvalidOperationException($"No protocol handler registered for command type: {commandType}");

            await handler.HandleAsync(client, data);
        }
    }
}
