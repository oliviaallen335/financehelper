using FinanceStudyHelper.Api.Contracts;
using FinanceStudyHelper.Api.Data;
using FinanceStudyHelper.Api.Models;
using FinanceStudyHelper.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceStudyHelper.Api.Controllers;

[ApiController]
[Route("api")]
public class ChatController(
    AppDbContext dbContext,
    RagService ragService,
    DeepSeekService deepSeekService,
    FunctionRouter functionRouter,
    NotesImportService notesImportService) : ControllerBase
{
    [HttpPost("chat")]
    public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Message cannot be empty.");
        }

        var session = await ResolveSessionAsync(request.SessionId);

        dbContext.ChatMessages.Add(new ChatMessage
        {
            SessionId = session.Id,
            Role = "user",
            Content = request.Message
        });
        await dbContext.SaveChangesAsync();

        var history = await dbContext.ChatMessages
            .Where(x => x.SessionId == session.Id)
            .OrderByDescending(x => x.Id)
            .Take(10)
            .OrderBy(x => x.Id)
            .Select(x => new ValueTuple<string, string>(x.Role, x.Content))
            .ToListAsync();

        var chunks = await ragService.RetrieveRelevantChunksAsync(request.Message);
        var ragContext = ragService.BuildContextBlock(chunks);

        var modelResult = await deepSeekService.ChatAsync(
            request.Message,
            ragContext,
            history,
            async (functionName, args) => await functionRouter.RunAsync(functionName, args, session.Id));

        dbContext.ChatMessages.Add(new ChatMessage
        {
            SessionId = session.Id,
            Role = "assistant",
            Content = modelResult.Content
        });
        await dbContext.SaveChangesAsync();

        return Ok(new ChatResponse(
            session.Id,
            modelResult.Content,
            ragService.ToCitations(chunks)));
    }

    [HttpPost("notes/import")]
    public async Task<ActionResult<object>> ImportNotes([FromBody] ImportNotesRequest request)
    {
        var count = await notesImportService.ImportFolderAsync(request.FolderPath);
        return Ok(new { importedChunks = count });
    }

    [HttpGet("lectures")]
    public async Task<ActionResult<object>> GetLectures()
    {
        var lectures = await dbContext.Lectures
            .OrderBy(x => x.Id)
            .Select(x => new { x.Id, x.Title, x.SourceFile, x.CreatedAt })
            .ToListAsync();
        return Ok(lectures);
    }

    [HttpGet("sessions/{sessionId:long}/messages")]
    public async Task<ActionResult<object>> GetSessionMessages([FromRoute] long sessionId)
    {
        var messages = await dbContext.ChatMessages
            .Where(x => x.SessionId == sessionId)
            .OrderBy(x => x.Id)
            .Select(x => new { x.Id, x.Role, x.Content, x.CreatedAt })
            .ToListAsync();
        return Ok(messages);
    }

    private async Task<ChatSession> ResolveSessionAsync(long? sessionId)
    {
        if (sessionId is long id)
        {
            var existing = await dbContext.ChatSessions.FindAsync(id);
            if (existing is not null)
            {
                return existing;
            }
        }

        var session = new ChatSession();
        dbContext.ChatSessions.Add(session);
        await dbContext.SaveChangesAsync();
        return session;
    }
}
