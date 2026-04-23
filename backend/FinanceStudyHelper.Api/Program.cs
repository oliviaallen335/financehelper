using FinanceStudyHelper.Api.Data;
using FinanceStudyHelper.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient("deepseek");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var rawConnectionString =
        builder.Configuration["JAWSDB_URL"]
        ?? builder.Configuration["MYSQL_URL"]
        ?? builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Missing ConnectionStrings:Default or JAWSDB_URL/MYSQL_URL.");

    var connectionString = NormalizeMySqlConnectionString(rawConnectionString);
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddScoped<RagService>();
builder.Services.AddScoped<FunctionRouter>();
builder.Services.AddScoped<DeepSeekService>();
builder.Services.AddScoped<NotesImportService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy
            .WithOrigins(
                "https://oliviaallen335.github.io",
                "http://localhost:5500",
                "http://127.0.0.1:5500")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();
app.Run();

static string NormalizeMySqlConnectionString(string raw)
{
    if (raw.StartsWith("mysql://", StringComparison.OrdinalIgnoreCase))
    {
        var uri = new Uri(raw);
        var userInfo = uri.UserInfo.Split(':', 2, StringSplitOptions.TrimEntries);
        var user = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "";
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var database = uri.AbsolutePath.Trim('/');
        var port = uri.Port > 0 ? uri.Port : 3306;
        return $"Server={uri.Host};Port={port};Database={database};User Id={user};Password={password};SslMode=Required;";
    }

    if (!raw.Contains("SslMode=", StringComparison.OrdinalIgnoreCase))
    {
        raw += raw.TrimEnd().EndsWith(';') ? "SslMode=Required;" : ";SslMode=Required;";
    }

    return raw;
}
