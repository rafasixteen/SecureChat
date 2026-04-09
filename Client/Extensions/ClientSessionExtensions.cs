using EI.SI;
using System.Text;

namespace Client.Extensions
{
    internal static class ClientSessionExtensions
    {
        public static ClientSession Ensure(this ClientSession? session)
        {
            if (session == null)
                throw new InvalidOperationException("No active session. Please connect to the server first.");

            return session;
        }

        public static void SendRegistrationPacket(this ClientSession session, string username, string password)
        {
            byte[] data = Encoding.UTF8.GetBytes($"{username}:{password}");
            session.SendEncryptedPacket(data, ProtocolSICmdType.USER_OPTION_1);
        }

        public static void SendLoginPacket(this ClientSession session, string username, string password)
        {
            byte[] data = Encoding.UTF8.GetBytes($"{username}:{password}");
            session.SendEncryptedPacket(data, ProtocolSICmdType.USER_OPTION_2);
        }

        public static void SendMessagePacket(this ClientSession session, string to, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes($"{to}:{message}");
            session.SendEncryptedPacket(data, ProtocolSICmdType.USER_OPTION_3);
        }
    }
}
