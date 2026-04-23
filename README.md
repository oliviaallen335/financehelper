# Finance Study Helper (PA3 Starter)

This starter includes:
- ASP.NET Core Web API (`backend/FinanceStudyHelper.Api`)
- DeepSeek integration (`DeepSeekService`)
- RAG retrieval over `lecture_chunks` (`RagService`)
- Function calling (`create_quiz`, `summarize_lecture`)
- Basic frontend (`frontend`)

## 1) Configure backend

Set environment variables (preferred):

- `DEEPSEEK_API_KEY=your_key_here`
- `DEEPSEEK_BASE_URL=https://api.deepseek.com`
- `DEEPSEEK_MODEL=deepseek-chat`

Set your MySQL connection string in:
- `backend/FinanceStudyHelper.Api/appsettings.json` under `ConnectionStrings:Default`

## 2) Create DB schema

Use your own SQL script in MySQL with tables:
- `lectures`
- `lecture_chunks`
- `chat_sessions`
- `chat_messages`
- `quizzes`

The entity mappings are in:
- `backend/FinanceStudyHelper.Api/Data/AppDbContext.cs`

## 3) Run backend

```bash
cd backend/FinanceStudyHelper.Api
dotnet restore
dotnet run
```

## 4) Import notes

Convert each finance lecture into `.txt` files and put them in a folder.

Call:

`POST /api/notes/import`

Body:

```json
{
  "folderPath": "C:\\path\\to\\your\\txt\\notes"
}
```

## 5) Run frontend

Open `frontend/index.html` in browser.

If backend port differs from `5194`, update `API_BASE` in `frontend/app.js`.

## 6) Main API endpoints

- `POST /api/chat`
- `POST /api/notes/import`
- `GET /api/lectures`
- `GET /api/sessions/{sessionId}/messages`
