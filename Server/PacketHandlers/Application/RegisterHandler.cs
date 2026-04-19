using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Data.Models;
using Server.Transport.Connection;
using Server.Transport.Security;
using Shared.DTOs;
using Shared.DTOs.Shared.DTOs;
using System.Net.Sockets;

namespace Server.PacketHandlers.Application
{
    public class RegisterHandler(ConnectionManager connectionManager) : IPacketHandler
    {
        private readonly ConnectionManager _connectionManager = connectionManager;

        // Handler de registo de utilizadores
        public async Task HandleAsync(TcpClient client, byte[] payload)
        {
            RegisterRequest request = Serializer.Deserialize<RegisterRequest>(payload);

            // Validar inputs
            if (!await ValidateInput(client, request))
                return;

            using AppDbContext db = new();

            // Verificar se já existe um utilizador com o mesmo username na base de dados
            bool userExists = await db.Users.AnyAsync(u => u.Username == request.Username);

            // Caso exista, enviar uma resposta de falha indicando que o username já está a ser utilizado
            if (userExists)
            {
                await Program.SendPacketAsync(client, "register-failed", "Username already taken.");
                return;
            }

            // Criar um hash e um salt para a password do utilizador utilizando a classe Password
            (string hash, string salt) = Password.Hash(request.Password);

            // Criar o utilizador e adicionar à base de dados, e guardar
            db.Users.Add(new User()
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = hash,
                Salt = salt
            });

            await db.SaveChangesAsync();

            // Definir o username no cliente para associar a conexão ao utilizador autenticado
            _connectionManager.SetUsername(client, request.Username);

            // Criar um RegisterResponse (Data Transfer Object) para enviar uma resposta de sucesso ao cliente
            RegisterResponse response = new(request.Username);
            byte[] responseData = Serializer.Serialize(response);

            // Enviar a resposta de sucesso
            await Program.SendPacketAsync(client, "register-success", responseData);
            Console.WriteLine($"[Server] User registered: {request.Username}");
        }

        // Verificar os inputs
        private static async Task<bool> ValidateInput(TcpClient client, RegisterRequest request)
        {
            // Verificar se o username ou password estão vazios ou consistem apenas em espaços em branco
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                await Program.SendPacketAsync(client, "register-failed", "Username and password are required.");
                return false;
            }

            // O nome é muito grande
            if (request.Username.Length > 32)
            {
                await Program.SendPacketAsync(client, "register-failed", "Username must be 32 characters or fewer.");
                return false;
            }

            // Password muito pequena
            if (request.Password.Length < 4)
            {
                await Program.SendPacketAsync(client, "register-failed", "Password must be at least 4 characters.");
                return false;
            }

            return true;
        }
    }
}
