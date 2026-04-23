using FinanceStudyHelper.Api.Contracts;
using FinanceStudyHelper.Api.Data;
using FinanceStudyHelper.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceStudyHelper.Api.Services;

public class RagService(AppDbContext dbContext)
{
    public async Task<List<LectureChunk>> RetrieveRelevantChunksAsync(string query, int topK = 5)
    {
        var terms = query
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length > 2)
            .Take(10)
            .ToList();

        if (terms.Count == 0)
        {
            return await dbContext.LectureChunks
                .Include(x => x.Lecture)
                .OrderByDescending(x => x.Id)
                .Take(topK)
                .ToListAsync();
        }

        var candidates = await dbContext.LectureChunks
            .Include(x => x.Lecture)
            .Take(5000)
            .ToListAsync();

        var ranked = candidates
            .Select(chunk => new
            {
                Chunk = chunk,
                Score = terms.Count(term =>
                    chunk.Content.Contains(term, StringComparison.OrdinalIgnoreCase))
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Chunk.Id)
            .Take(topK)
            .Select(x => x.Chunk)
            .ToList();

        return ranked;
    }

    public string BuildContextBlock(IEnumerable<LectureChunk> chunks)
    {
        var parts = chunks.Select(chunk =>
            $"[ChunkId:{chunk.Id} | LectureId:{chunk.LectureId} | Title:{chunk.Lecture?.Title ?? "Unknown"}]\n{chunk.Content}");
        return string.Join("\n\n---\n\n", parts);
    }

    public List<CitationDto> ToCitations(IEnumerable<LectureChunk> chunks)
    {
        return chunks
            .Select(x => new CitationDto(x.Id, x.LectureId, x.Lecture?.Title ?? "Unknown"))
            .ToList();
    }
}
