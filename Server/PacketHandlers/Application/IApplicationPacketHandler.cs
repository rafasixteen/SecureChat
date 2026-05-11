namespace Server.PacketHandlers.Application
{
    public interface IApplicationPacketHandler : IPacketHandler
    {
        string CommandType { get; }
    }
}