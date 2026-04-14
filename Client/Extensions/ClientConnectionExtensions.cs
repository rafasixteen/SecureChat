using Client.Transport;
using Shared.DTOs;

namespace Client.Extensions
{
    internal static class ClientConnectionExtensions
    {
        public static ClientConnection Ensure(this ClientConnection? connection)
        {
            if (connection == null)
                throw new InvalidOperationException("No active connection. Please connect to the server first.");

            return connection;
        }

        public static async Task SendRegistrationPacketAsync(this ClientConnection session, string username, string password)
        {
            byte[] payload = Serializer.Serialize(new RegisterRequest(username, password));
            await session.SendPacketAsync("register", payload);
        }

        public static async Task SendLoginPacketAsync(this ClientConnection session, string username, string password)
        {
            byte[] payload = Serializer.Serialize(new LoginRequest(username, password));
            await session.SendPacketAsync("login", payload);
        }

        public static async Task RequestFriendsList(this ClientConnection session)
        {
            await session.SendPacketAsync("get-friends", []);
        }
    }
}
