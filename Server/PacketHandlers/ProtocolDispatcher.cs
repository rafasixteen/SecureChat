using EI.SI;
using System.Net.Sockets;

namespace Server.PacketHandlers
{
    public class ProtocolDispatcher
    {
        // Dicionário para mapear os tipos de comando para seus respectivos manipuladores
        private readonly Dictionary<ProtocolSICmdType, IPacketHandler> _handlers = new();

        // Método para registar um manipulador para um tipo de comando específico
        public ProtocolDispatcher With(ProtocolSICmdType commandType, IPacketHandler handler)
        {
            if (!_handlers.TryAdd(commandType, handler))
                throw new InvalidOperationException($"Protocol handler for command type '{commandType}' is already registered.");

            return this;
        }

        // Método para encaminhar um comando recebido para o manipulador apropriado
        public async Task DispatchAsync(TcpClient client, ProtocolSICmdType commandType, byte[] data)
        {
            if (!_handlers.TryGetValue(commandType, out IPacketHandler? handler))
                throw new InvalidOperationException($"No protocol handler registered for command type: {commandType}");

            await handler.HandleAsync(client, data);
        }
    }
}
