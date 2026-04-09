using EI.SI;

namespace Server.PacketHandlers
{
    internal class PacketHandlerFactory
    {
        private readonly Dictionary<ProtocolSICmdType, IPacketHandler> _handlers;

        public PacketHandlerFactory()
        {
            _handlers = [];
        }

        public void Register(IPacketHandler handler)
        {
            _handlers[handler.CommandType] = handler;
        }

        public IPacketHandler? GetHandler(ProtocolSICmdType type)
        {
            if (!_handlers.TryGetValue(type, out IPacketHandler? handler))
            {
                throw new Exception("No handler registered for command type: " + type);
            }

            return handler;
        }
    }
}
