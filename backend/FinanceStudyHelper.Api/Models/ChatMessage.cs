namespace FinanceStudyHelper.Api.Models;

public class ChatMessage
{
    public long Id { get; set; }
    public long SessionId { get; set; }
    public string Role { get; set; } = "user";
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ChatSession? Session { get; set; }
}
