using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Server.Transport.Connection
{
    public class ConnectionManager
    {
        private ConcurrentDictionary<TcpClient, Connection> ConnectedClients { get; } = new();

        public void Connect(TcpClient client)
        {
            Connection connection = new();

            if (!ConnectedClients.TryAdd(client, connection))
                throw new Exception("Failed to add client to connection manager.");

            Console.WriteLine($"[Server] Client connected: {client.Client.RemoteEndPoint}");
        }

        public void Disconnect(TcpClient client)
        {
            Console.WriteLine($"[Server] Client disconnected: {client.Client.RemoteEndPoint}");

            ConnectedClients.TryRemove(client, out _);
            client.Close();
        }

        public bool IsAuthenticated(TcpClient client)
        {
            Connection connection = GetConnection(client);
            return connection.Username != null;
        }

        public void SetAesKeys(TcpClient client, byte[] aesKey, byte[] aesIv)
        {
            Connection connection = GetConnection(client);

            if (connection.AesKey != null && connection.AesIv != null)
                throw new Exception("Handshake already completed.");

            connection.AesKey = aesKey;
            connection.AesIv = aesIv;
        }

        public (byte[] aesKey, byte[] aesIv) GetAesKeys(TcpClient client)
        {
            Connection connection = GetConnection(client);

            if (connection.AesKey == null || connection.AesIv == null)
                throw new Exception("Handshake not completed.");

            return (connection.AesKey!, connection.AesIv!);
        }

        public void SetUsername(TcpClient client, string username)
        {
            Connection connection = GetConnection(client);

            if (connection.AesKey == null || connection.AesIv == null)
                throw new Exception("Handshake not completed.");

            if (connection.Username != null)
                throw new Exception("Already authenticated.");

            connection.Username = username;
        }

        public string GetUsername(TcpClient client)
        {
            Connection connection = GetConnection(client);

            if (connection.Username == null)
                throw new Exception("Not authenticated.");

            return connection.Username;
        }

        private Connection GetConnection(TcpClient client)
        {
            if (!ConnectedClients.TryGetValue(client, out Connection? connection))
                throw new Exception("Client not connected.");

            return connection;
        }

        private class Connection
        {
            public byte[]? AesKey;

            public byte[]? AesIv;

            public string? Username;
        }
    }
}
