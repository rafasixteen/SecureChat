namespace Shared
{
    public record Envelope(string CommandType, byte[] Payload, byte[]? Signature = null);
}
