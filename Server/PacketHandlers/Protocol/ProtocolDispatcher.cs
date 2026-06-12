using EI.SI;
using System.Net.Sockets;

namespace Server.PacketHandlers.Protocol
{
    public class ProtocolDispatcher
    {
        private readonly Dictionary<ProtocolSICmdType, IPacketHandler> _handlers;

        public ProtocolDispatcher(IEnumerable<IProtocolPacketHandler> handlers)
        {
            _handlers = handlers.ToDictionary(h => h.CommandType, h => (IPacketHandler)h);
        }

        /// <summary>
        /// Triggers events based on communication type
        /// </summary>
        public async Task DispatchAsync(TcpClient client, ProtocolSICmdType commandType, byte[] data)
        {
            if (!_handlers.TryGetValue(commandType, out IPacketHandler? handler))
                throw new InvalidOperationException($"No protocol handler registered for command type: {commandType}");

            await handler.HandleAsync(client, data);
        }
    }
}
