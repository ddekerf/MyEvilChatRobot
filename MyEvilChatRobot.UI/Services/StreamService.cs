using System.Text;

namespace MyEvilChatRobot.UI.Services;

public class StreamService
{
    private readonly HttpClient _httpClient;
    private const string ApiUrl = "https://localhost:7189/api/MyEvilChatbot/stream"; // API URL

    public StreamService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task StreamResponse(string userInput, Func<string, Task> onMessageReceived)
    {
        var cleanedUserInput = userInput.Trim().Replace("\n", "").Replace("\r", "");
        var inputContent = new StringContent($"{{\"inputText\":\"{cleanedUserInput}\"}}", Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
        {
            Content = inputContent
        };

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (await reader.ReadLineAsync() is { } line)
        {
            if (!line.StartsWith("data: ")) continue;
            var responseChunk = line[6..];
            await onMessageReceived(responseChunk);
        }
    }
}