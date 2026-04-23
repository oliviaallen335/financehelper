namespace FinanceStudyHelper.Api.Contracts;

public record ChatRequest(long? SessionId, string Message);

public record CitationDto(long ChunkId, int LectureId, string LectureTitle);

public record ChatResponse(long SessionId, string Reply, List<CitationDto> Citations);

public record ImportNotesRequest(string FolderPath);
