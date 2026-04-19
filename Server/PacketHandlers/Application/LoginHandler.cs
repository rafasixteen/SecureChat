using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Data.Models;
using Server.Transport.Connection;
using Server.Transport.Security;
using Shared.DTOs;
using System.Net.Sockets;

namespace Server.PacketHandlers.Application
{
    public class LoginHandler(ConnectionManager connectionManager) : IPacketHandler
    {
        private readonly ConnectionManager _connectionManager = connectionManager;

        // Processar um pedido de login
        public async Task HandleAsync(TcpClient client, byte[] payload)
        {
            LoginRequest request = Serializer.Deserialize<LoginRequest>(payload);

            if (!await ValidateInput(client, request))
                return;

            using AppDbContext db = new();

            // Obter o utilizador da base de dados através da EF Core
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            // Verificar se as credenciais que o utilizador inserio estão corretas
            bool valid = user != null && Password.Verify(request.Password, user.PasswordHash, user.Salt);

            if (!valid)
            {
                // Se não estiverem, indica que uma das duas credenciais está incorreta.
                await Program.SendPacketAsync(client, "login-failed", "Invalid username or password.");
                return;
            }

            // Definir o username no cliente para associar a conexão ao utilizador autenticado
            _connectionManager.SetUsername(client, request.Username);

            // Criar um LoginResponse (Data Transfer Object) para enviar uma resposta ao cliente
            LoginResponse response = new(request.Username);
            byte[] responseData = Serializer.Serialize(response);

            // Enviar a resposta de sucesso
            await Program.SendPacketAsync(client, "login-success", responseData);
            Console.WriteLine($"[Server] User logged in: {request.Username}");

        }

        // Valida o LoginRequest para garantir que os campos necessários estejam corretos (Não vazios)
        private static async Task<bool> ValidateInput(TcpClient client, LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                // Enviar uma resposta de falha se o username ou password estiverem vazios ou consistirem apenas em espaços em branco
                await Program.SendPacketAsync(client, "login-failed", "Username and password are required.");
                return false;
            }

            return true;
        }
    }
}
