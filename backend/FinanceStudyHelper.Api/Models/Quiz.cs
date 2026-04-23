namespace FinanceStudyHelper.Api.Models;

public class Quiz
{
    public long Id { get; set; }
    public long SessionId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string QuizJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ChatSession? Session { get; set; }
}
