using ProtoIP;

namespace SecureChat.Client
{
    /// <summary>
    /// Extends ProtoClient to handle chat-specific receive logic.
    /// </summary>
    public class ChatClient() : ProtoClient
    {
        /// <summary>
        /// Action to update the UI when a message arrives.
        /// </summary>
        public event Action<string>? MessageReceived;

        /// <summary>
        /// Action to open the chat form when authentication succeeds.
        /// </summary>
        public event Action? AuthenticationSucceeded;

        /// <summary>
        /// Called when data is received from the server.
        /// Gets the raw data, deserializes it, and fires the UI callback.
        /// </summary>
        public override void OnReceive()
        {
            Packet packet = AssembleReceivedDataIntoPacket();

            switch ((Packet.Type)packet._GetType())
            {
                case Packet.Type.ACK:
                    AuthenticationSucceeded?.Invoke();
                    return;
                case Packet.Type.BYTES:
                    string message = packet.GetDataAs<string>();
                    MessageReceived?.Invoke(message);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Sends a chat message to the server as a BYTES packet.
        /// </summary>
        public void SendMessage(string message)
        {
            Packet packet = new(Packet.Type.BYTES);
            packet.SetPayload(message);
            Send(Packet.Serialize(packet));
        }
    }
}