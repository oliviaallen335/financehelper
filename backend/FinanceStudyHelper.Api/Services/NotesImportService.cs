using FinanceStudyHelper.Api.Data;
using FinanceStudyHelper.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceStudyHelper.Api.Services;

public class NotesImportService(AppDbContext dbContext)
{
    public async Task<int> ImportFolderAsync(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            throw new DirectoryNotFoundException($"Folder not found: {folderPath}");
        }

        var files = Directory.GetFiles(folderPath, "*.txt");
        var importedChunks = 0;

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var title = Path.GetFileNameWithoutExtension(file);
            var text = await File.ReadAllTextAsync(file);
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            var existing = await dbContext.Lectures
                .FirstOrDefaultAsync(x => x.SourceFile == fileName);
            if (existing is not null)
            {
                continue;
            }

            var lecture = new Lecture
            {
                Title = title,
                SourceFile = fileName
            };
            dbContext.Lectures.Add(lecture);
            await dbContext.SaveChangesAsync();

            var chunks = ChunkText(text, 800, 120)
                .Select((content, idx) => new LectureChunk
                {
                    LectureId = lecture.Id,
                    ChunkIndex = idx,
                    Content = content
                })
                .ToList();

            dbContext.LectureChunks.AddRange(chunks);
            importedChunks += chunks.Count;
            await dbContext.SaveChangesAsync();
        }

        return importedChunks;
    }

    private static IEnumerable<string> ChunkText(string content, int size, int overlap)
    {
        var normalized = content.Replace("\r\n", "\n").Trim();
        if (normalized.Length <= size)
        {
            yield return normalized;
            yield break;
        }

        var step = Math.Max(1, size - overlap);
        for (var i = 0; i < normalized.Length; i += step)
        {
            var len = Math.Min(size, normalized.Length - i);
            var chunk = normalized.Substring(i, len).Trim();
            if (!string.IsNullOrWhiteSpace(chunk))
            {
                yield return chunk;
            }
            if (i + len >= normalized.Length)
            {
                break;
            }
        }
    }
}
