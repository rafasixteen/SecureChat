using System.Net.Sockets;

namespace Server.PacketHandlers
{
    public class ApplicationDispatcher
    {
        private readonly Dictionary<string, IPacketHandler> _handlers = new();

        // Regista um manipulador para um tipo de comando específico.
        public ApplicationDispatcher With(string commandType, IPacketHandler handler)
        {
            // Lança uma exceção se um manipulador já estiver registado para o mesmo tipo de comando.
            if (!_handlers.TryAdd(commandType, handler))
                throw new InvalidOperationException($"Application handler for command type '{commandType}' is already registered.");

            // Retorna o próprio dispatcher para permitir encadeamento de chamadas (fluent interface).
            return this;
        }

        // Dispara o manipulador registado para o tipo de comando especificado, passando o cliente e os dados.
        public async Task DispatchAsync(TcpClient client, string commandType, byte[] data)
        {
            if (!_handlers.TryGetValue(commandType, out IPacketHandler? handler))
                throw new InvalidOperationException($"No application handler registered for command type: {commandType}");

            // Chama o manipulador assíncrono para processar os dados recebidos do cliente.
            await handler.HandleAsync(client, data);
        }
    }
}
