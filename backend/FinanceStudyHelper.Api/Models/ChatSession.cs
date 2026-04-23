namespace FinanceStudyHelper.Api.Models;

public class ChatSession
{
    public long Id { get; set; }
    public string? UserLabel { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<ChatMessage> Messages { get; set; } = [];
}
