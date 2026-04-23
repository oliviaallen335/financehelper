using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FinanceStudyHelper.Api.Contracts;

namespace FinanceStudyHelper.Api.Services;

public class DeepSeekService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
{
    private readonly string _apiKey = configuration["DEEPSEEK_API_KEY"] ?? string.Empty;
    private readonly string _baseUrl = configuration["DEEPSEEK_BASE_URL"] ?? configuration["DeepSeek:BaseUrl"] ?? "https://api.deepseek.com";
    private readonly string _model = configuration["DEEPSEEK_MODEL"] ?? configuration["DeepSeek:Model"] ?? "deepseek-chat";

    public async Task<DeepSeekChatResult> ChatAsync(
        string userPrompt,
        string ragContext,
        List<(string Role, string Content)> history,
        Func<string, JsonElement, Task<string>> onToolCall)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException("Missing DEEPSEEK_API_KEY environment variable.");
        }

        var messages = new List<object>
        {
            new
            {
                role = "system",
                content =
                    "You are a finance study helper. Prefer lecture context for factual claims. If missing context, explicitly say it."
            }
        };

        messages.AddRange(history.Select(h => new { role = h.Role, content = h.Content }));
        messages.Add(new
        {
            role = "system",
            content = $"Retrieved lecture context:\n{ragContext}"
        });
        messages.Add(new { role = "user", content = userPrompt });

        var tools = BuildTools();
        var firstResponse = await SendChatRequestAsync(messages, tools);
        var firstAssistantMessage = firstResponse.GetProperty("choices")[0].GetProperty("message");

        if (firstAssistantMessage.TryGetProperty("tool_calls", out var toolCalls) &&
            toolCalls.ValueKind == JsonValueKind.Array &&
            toolCalls.GetArrayLength() > 0)
        {
            var mutableMessages = new List<object>(messages)
            {
                new { role = "assistant", content = firstAssistantMessage.GetProperty("content").GetString(), tool_calls = toolCalls }
            };

            foreach (var toolCall in toolCalls.EnumerateArray())
            {
                var function = toolCall.GetProperty("function");
                var functionName = function.GetProperty("name").GetString() ?? string.Empty;
                var argsString = function.GetProperty("arguments").GetString() ?? "{}";
                using var argsDoc = JsonDocument.Parse(argsString);
                var result = await onToolCall(functionName, argsDoc.RootElement);

                mutableMessages.Add(new
                {
                    role = "tool",
                    tool_call_id = toolCall.GetProperty("id").GetString(),
                    content = result
                });
            }

            var secondResponse = await SendChatRequestAsync(mutableMessages, tools);
            var finalMessage = secondResponse.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()
                               ?? "No content returned.";
            return new DeepSeekChatResult(finalMessage);
        }

        var directContent = firstAssistantMessage.GetProperty("content").GetString() ?? "No content returned.";
        return new DeepSeekChatResult(directContent);
    }

    private async Task<JsonElement> SendChatRequestAsync(List<object> messages, object tools)
    {
        var client = httpClientFactory.CreateClient("deepseek");
        client.BaseAddress = new Uri(_baseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var payload = new
        {
            model = _model,
            messages,
            tools,
            tool_choice = "auto",
            temperature = 0.2
        };

        var body = JsonSerializer.Serialize(payload);
        using var response = await client.PostAsync(
            "/chat/completions",
            new StringContent(body, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        var raw = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(raw);
        return doc.RootElement.Clone();
    }

    private static object BuildTools()
    {
        return new object[]
        {
            new
            {
                type = "function",
                function = new
                {
                    name = "create_quiz",
                    description = "Create a study quiz for a finance topic.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            topic = new { type = "string" },
                            numQuestions = new { type = "integer", minimum = 1, maximum = 15 }
                        },
                        required = new[] { "topic", "numQuestions" }
                    }
                }
            },
            new
            {
                type = "function",
                function = new
                {
                    name = "summarize_lecture",
                    description = "Summarize one lecture by lectureId.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            lectureId = new { type = "integer" }
                        },
                        required = new[] { "lectureId" }
                    }
                }
            }
        };
    }
}

public record DeepSeekChatResult(string Content);
