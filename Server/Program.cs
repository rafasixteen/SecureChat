using EI.SI;
using Server.PacketHandlers;
using Server.PacketHandlers.Application;
using Server.PacketHandlers.Protocol;
using Server.Transport.Connection;
using Server.Transport.Security;
using Shared;
using Shared.DTOs;
using Shared.Exceptions;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Server
{
    internal static class Program
    {
        // Porta fixa para o servidor.
        public const int PORT = 8080;

        // Chave RSA para o servidor, gerada na inicialização, 2048 bits para segurança adequada.
        public static readonly RSA Rsa = RSA.Create(2048);

        // Gerenciador de conexões para manter o estado dos clientes conectados, incluindo chaves AES.
        private static readonly ConnectionManager _connectionManager = new();

        // Dispatcher para comandos de protocolo, mapeando tipos de comando para manipuladores específicos.
        private static readonly ProtocolDispatcher _protocolDispacter = new();

        // Dispatcher para comandos de aplicação, mapeando strings de comando para manipuladores de lógica de negócios.
        private static readonly ApplicationDispatcher _applicationDispatcher = new();

        public static async Task Main(string[] args)
        {
            Console.WriteLine($"[Server] Starting on port {PORT}...");

            // Configura os manipuladores de protocolo para os tipos de comando específicos, associando-os ao gerenciador de conexões e ao dispatcher de aplicação.
            _protocolDispacter.With(ProtocolSICmdType.SYM_CIPHER_DATA, new SymmetricDataHandler(_connectionManager, _applicationDispatcher));
            _protocolDispacter.With(ProtocolSICmdType.SECRET_KEY, new SecretKeyHandler(_connectionManager, Rsa));

            // Configura os manipuladores de aplicação para os comandos específicos
            _applicationDispatcher.With("register", new RegisterHandler(_connectionManager));
            _applicationDispatcher.With("login", new LoginHandler(_connectionManager));
            _applicationDispatcher.With("get-friends", new FriendsListHandler(_connectionManager));
            _applicationDispatcher.With("get-conversation", new GetConversationHandler(_connectionManager));
            _applicationDispatcher.With("send-message", new MessageHandler(_connectionManager));

            // Token de cancelamento para permitir desligar o servidor de forma graciosa quando o usuário pressionar ENTER.
            // Desligar um servidor de forma graciosa significa: Desligar com muito cuidado, garantindo que todas as conexões sejam fechadas corretamente e que os recursos sejam liberados, evitando perda de dados ou corrupção.
            using CancellationTokenSource cts = new();

            // Criar um Listener de TCP e iniciar
            TcpListener listener = new(IPAddress.Any, PORT);
            listener.Start();

            Console.WriteLine("[Server] Listening...");
            Console.WriteLine("[Server] Press ENTER to shut down.");

            // Iniciar uma tarefa em segundo plano para aguardar o pressionamento de ENTER e solicitar que o servidor seja desligado.
            _ = Task.Run(() =>
            {
                Console.ReadLine();
                Console.WriteLine("[Server] Shutdown requested...");
                cts.Cancel();
            });

            // Loop principal do servidor para aceitar conexões de clientes. O loop continua até que o token de cancelamento seja acionado.
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    // Aceitar uma conexão de cliente de forma assíncrona. O método AcceptTcpClientAsync é usado para não bloquear o thread principal do servidor enquanto aguarda por conexões.
                    TcpClient client = await listener.AcceptTcpClientAsync(cts.Token);
                    _connectionManager.Connect(client);
                    _ = Task.Run(() => HandleClientAsync(client), cts.Token);
                }
            }
            // Captura a exceção de cancelamento que ocorre quando o token de cancelamento é acionado, indicando que o servidor deve ser desligado.
            catch (OperationCanceledException)
            {
                Console.WriteLine("[Server] Shutdown signal received.");
            }
            // Captura qualquer outra exceção inesperada que possa ocorrer durante a execução do servidor, registrando a mensagem de erro para diagnóstico.
            catch (Exception ex)
            {
                Console.WriteLine($"[Server] Unexpected error: {ex.Message}");
            }
            // O bloco finally é executado independentemente de como o try/catch foi concluído, garantindo que o listener seja parado e que uma mensagem de desligamento seja exibida, mesmo que ocorra um erro inesperado.
            finally
            {
                listener.Stop();
                Console.WriteLine("[Server] Server stopped.");
            }
        }

        #region Client Handling

        // Função assíncrona para lidar com a comunicação de um cliente específico.
        // Esta função é executada em uma tarefa separada para cada cliente conectado, permitindo que o servidor atenda a múltiplos clientes simultaneamente.
        private static async Task HandleClientAsync(TcpClient client)
        {
            // Utilizar o ProtocolSI para gerir a comunicação com o cliente, incluindo a construção e interpretação de pacotes de dados.
            ProtocolSI protocol = new();
            NetworkStream stream = client.GetStream();

            try
            {
                await Handshake.SendPublicKey(client, Rsa);

                // Iniciar o loop de recepção de dados do cliente, onde o servidor aguarda por mensagens do cliente e as processa usando o dispatcher de protocolo.
                await ReceiveLoopAsync(client, protocol, stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Server] Client error {client.Client.RemoteEndPoint}: {ex.Message}");
            }
            finally
            {
                // Desconectar o cliente
                _connectionManager.Disconnect(client);
            }
        }

        // Loop assíncrono para receber e processar mensagens de um cliente conectado. O loop continua enquanto o cliente estiver conectado.
        private static async Task ReceiveLoopAsync(TcpClient client, ProtocolSI protocol, NetworkStream stream)
        {
            while (client.Connected)
            {
                // Ler dados do cliente de forma assíncrona
                int bytesRead = await stream.ReadAsync(protocol.Buffer.AsMemory(0, protocol.Buffer.Length));

                // Parar o loop se o cliente desconectar (bytesRead == 0 indica que a conexão foi fechada pelo cliente)
                if (bytesRead == 0)
                    break;

                // Ler o tipo de comando e os dados do pacote através do ProtocolSI
                ProtocolSICmdType commandType = protocol.GetCmdType();
                byte[] payload = protocol.GetData();

                try
                {
                    // Enviar o comando para o dispatcher
                    await _protocolDispacter.DispatchAsync(client, commandType, payload);
                }
                catch (InvalidPacketException ex)
                {
                    // Disconectar o cliente se o servidor receber um pacote inválido
                    Console.WriteLine($"[Server] Invalid packet from {client.Client.RemoteEndPoint}: {ex.Message}");
                    _connectionManager.Disconnect(client);
                    break;
                }
                catch (Exception ex)
                {
                    // Enviar mensagem de erro caso algo não esperado aconteça
                    Console.WriteLine($"[Server] {commandType} error: {ex}");
                    await SendPacketAsync(client, "server-failed", "Internal server error");
                }
            }
        }

        #endregion

        #region Packet Sending

        // Função assíncrona para enviar pacotes para clientes específicos.
        // Serve para enviar mensagens de volta para o cliente.
        public static async Task SendPacketAsync(TcpClient client, ProtocolSICmdType commandType, byte[] payload)
        {
            await SendRawAsync(client, commandType, payload).ConfigureAwait(false);
        }

        // Função assíncrona para enviar pacotes de texto para o cliente, convertendo a string em bytes UTF-8 antes de enviar.
        public static async Task SendPacketAsync(TcpClient client, ProtocolSICmdType commandType, string message)
        {
            byte[] payload = Encoding.UTF8.GetBytes(message);
            await SendPacketAsync(client, commandType, payload).ConfigureAwait(false);
        }

        // Função assíncrona para enviar pacotes de dados de aplicação para o cliente, serializando um objeto Envelope em bytes antes de enviar.
        public static async Task SendPacketAsync(TcpClient client, string commandType, byte[] payload)
        {
            Envelope env = new(commandType, payload);
            byte[] data = Serializer.Serialize(env);

            await SendPacketAsync(client, ProtocolSICmdType.SYM_CIPHER_DATA, data).ConfigureAwait(false);
        }

        // Função assíncrona para enviar pacotes de texto de aplicação para o cliente, convertendo a string em bytes UTF-8 e serializando um objeto Envelope antes de enviar.
        public static async Task SendPacketAsync(TcpClient client, string commandType, string message)
        {
            byte[] payload = Encoding.UTF8.GetBytes(message);
            await SendPacketAsync(client, commandType, payload).ConfigureAwait(false);
        }

        // Função assíncrona para enviar pacotes de dados brutos para o cliente, criptografando o payload com AES antes de enviar.
        // Esta função é usada internamente para garantir que os dados sejam enviados de forma segura.
        private static async Task SendRawAsync(TcpClient client, ProtocolSICmdType commandType, byte[] payload)
        {
            // Obter as chaves AES para o cliente específico
            (byte[] aesKey, byte[] aesIv) = _connectionManager.GetAesKeys(client);

            ProtocolSI protocol = new();
            NetworkStream stream = client.GetStream();

            // Encriptar com AES e fazer um packet com o ProtocolSI
            byte[] encrypted = AesUtils.Encrypt(payload, aesKey, aesIv);
            byte[] packet = protocol.Make(commandType, encrypted);

            // Enviar o packet para o cliente
            await stream.WriteAsync(packet).ConfigureAwait(false);
        }

        #endregion
    }
}