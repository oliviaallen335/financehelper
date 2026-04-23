namespace FinanceStudyHelper.Api.Models;

public class Lecture
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SourceFile { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<LectureChunk> Chunks { get; set; } = [];
}
