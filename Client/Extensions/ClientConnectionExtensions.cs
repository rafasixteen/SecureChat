using Client.Transport;
using Shared.DTOs;

namespace Client.Extensions
{
    internal static class ClientConnectionExtensions
    {
        public static async Task SendRegistrationPacketAsync(this ClientConnection session, string username, string password)
        {
            byte[] payload = Serializer.Serialize(new RegisterRequest(username, password));
            await session.SendPacketAsync("register", payload);
            Console.WriteLine("[Client] Sending registration packet for user: {0}", username);
        }

        public static async Task SendLoginPacketAsync(this ClientConnection session, string username, string password)
        {
            byte[] payload = Serializer.Serialize(new LoginRequest(username, password));
            await session.SendPacketAsync("login", payload);
            Console.WriteLine("[Client] Sending login packet for user: {0}", username);
        }

        public static async Task RequestFriendsList(this ClientConnection session)
        {
            await session.SendPacketAsync("get-friends", []);
            Console.WriteLine("[Client] Requesting friends list");
        }

        public static async Task RequestConversation(this ClientConnection session, string friendUsername)
        {
            byte[] payload = Serializer.Serialize(new GetConversationRequest(friendUsername));
            await session.SendPacketAsync("get-conversation", payload);

            Console.WriteLine("[Client] Requesting conversation with {0}", friendUsername);
        }

        public static async Task SendMessage(this ClientConnection session, string friendUsername, string message)
        {
            byte[] payload = Serializer.Serialize(new SendMessageRequest(friendUsername, message));
            await session.SendPacketAsync("send-message", payload);

            Console.WriteLine("[Client] Sending message to {0}: {1}", friendUsername, message);
        }
    }
}