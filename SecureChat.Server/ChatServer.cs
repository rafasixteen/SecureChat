using ProtoIP;

namespace SecureChat.Server
{
    /// <summary>
    /// Handles all client connections by extending ProtoServer.
    /// Each client gets its own thread automatically via AcceptConnections().
    /// </summary>
    public class ChatServer : ProtoServer
    {
        /// <summary>
        /// Called every time data is received from a client.
        /// Deserializes the packet and broadcasts the message to all other clients.
        /// </summary>
        public override void OnRequest(int userId)
        {
            try
            {
                // Get raw bytes and deserialize into a Packet.
                byte[] data = _clients[userId].GetDataAs<byte[]>();
                Packet packet = Packet.Deserialize(data);

                Packet.Type packetType = (Packet.Type)packet._GetType();

                switch (packetType)
                {
                    case Packet.Type.BYTES:
                        string message = packet.GetDataAs<string>();
                        Console.WriteLine($"[Server] Client {userId}: {message}");

                        // Broadcast to all other connected clients.
                        BroadcastToOthers(userId, message);
                        break;
                    case Packet.Type.HANDSHAKE_REQ:
                        // Send the ACK packet to simulate the user was authorized.
                        Send(Packet.Serialize(new Packet(Packet.Type.ACK)), userId);

                        Console.WriteLine($"[Server] Client {userId} authenticated.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Server] Error handling client {userId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a message to all clients except the sender.
        /// </summary>
        private void BroadcastToOthers(int senderId, string message)
        {
            for (int i = 0; i < _clients.Count; i++)
            {
                if (i == senderId) continue;

                if (!_clients[i].IsConnected()) continue;

                try
                {
                    Packet outPacket = new(Packet.Type.BYTES);
                    outPacket.SetPayload(message);
                    Send(Packet.Serialize(outPacket), i);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Server] Failed sending to client {i}: {ex.Message}");
                }
            }
        }
    }
}