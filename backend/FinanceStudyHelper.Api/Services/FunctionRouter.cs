using System.Text.Json;
using FinanceStudyHelper.Api.Data;
using FinanceStudyHelper.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceStudyHelper.Api.Services;

public class FunctionRouter(AppDbContext dbContext, RagService ragService)
{
    public async Task<string> RunAsync(string functionName, JsonElement args, long sessionId)
    {
        return functionName switch
        {
            "create_quiz" => await CreateQuizAsync(args, sessionId),
            "summarize_lecture" => await SummarizeLectureAsync(args),
            _ => $"Unknown function: {functionName}"
        };
    }

    private async Task<string> CreateQuizAsync(JsonElement args, long sessionId)
    {
        var topic = args.TryGetProperty("topic", out var topicEl)
            ? topicEl.GetString() ?? "General Finance"
            : "General Finance";
        var numQuestions = args.TryGetProperty("numQuestions", out var nEl)
            ? Math.Clamp(nEl.GetInt32(), 1, 15)
            : 5;

        var chunks = await ragService.RetrieveRelevantChunksAsync(topic, topK: Math.Min(8, numQuestions + 2));
        var random = new Random();

        var questions = chunks
            .Take(numQuestions)
            .Select((chunk, idx) => new
            {
                questionNumber = idx + 1,
                question = $"Based on lecture notes, explain this concept: {topic}. Include detail from Chunk {chunk.Id}.",
                sourceChunkId = chunk.Id
            })
            .ToList();

        while (questions.Count < numQuestions)
        {
            questions.Add(new
            {
                questionNumber = questions.Count + 1,
                question = $"Define and apply a key idea related to {topic}.",
                sourceChunkId = 0L
            });
        }

        var quizPayload = JsonSerializer.Serialize(new
        {
            topic,
            numQuestions,
            generatedAt = DateTime.UtcNow,
            seed = random.Next(1000, 9999),
            questions
        });

        dbContext.Quizzes.Add(new Quiz
        {
            SessionId = sessionId,
            Topic = topic,
            QuizJson = quizPayload
        });
        await dbContext.SaveChangesAsync();

        return quizPayload;
    }

    private async Task<string> SummarizeLectureAsync(JsonElement args)
    {
        if (!args.TryGetProperty("lectureId", out var lectureIdEl))
        {
            return "lectureId is required";
        }

        var lectureId = lectureIdEl.GetInt32();
        var chunks = await dbContext.LectureChunks
            .Where(x => x.LectureId == lectureId)
            .OrderBy(x => x.ChunkIndex)
            .Take(20)
            .ToListAsync();

        if (chunks.Count == 0)
        {
            return $"No chunks found for lectureId={lectureId}";
        }

        var merged = string.Join(" ", chunks.Select(x => x.Content));
        var summary = merged.Length > 1600 ? merged[..1600] : merged;
        return $"Lecture {lectureId} summary source (truncated): {summary}";
    }
}
