namespace FinanceStudyHelper.Api.Models;

public class LectureChunk
{
    public long Id { get; set; }
    public int LectureId { get; set; }
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Embedding { get; set; }
    public Lecture? Lecture { get; set; }
}
