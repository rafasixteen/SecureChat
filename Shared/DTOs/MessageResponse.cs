namespace Shared.DTOs
{
    public record MessageResponse(string Content, DateTime SentAt, bool Received);
}