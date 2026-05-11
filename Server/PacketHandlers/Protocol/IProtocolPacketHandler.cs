using EI.SI;

namespace Server.PacketHandlers.Protocol
{
    public interface IProtocolPacketHandler : IPacketHandler
    {
        ProtocolSICmdType CommandType { get; }
    }
}