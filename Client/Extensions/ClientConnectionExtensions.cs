using EI.SI;
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
            byte[] data = Serializer.Serialize(new RegisterRequest(username, password));
            await session.SendPacketAsync(data, ProtocolSICmdType.USER_OPTION_1);
        }

        public static async Task SendLoginPacketAsync(this ClientConnection session, string username, string password)
        {
            byte[] data = Serializer.Serialize(new LoginRequest(username, password));
            await session.SendPacketAsync(data, ProtocolSICmdType.USER_OPTION_2);
        }

        public static async Task RequestFriendsList(this ClientConnection session)
        {
            await session.SendPacketAsync([], ProtocolSICmdType.USER_OPTION_3);
        }
    }
}
