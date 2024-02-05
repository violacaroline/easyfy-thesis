using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;


public class OpenAIService
{
   private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public OpenAIService(HttpClient httpClient, IOptions<OpenAIServiceOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        var settings = options.Value ?? throw new ArgumentNullException(nameof(options));
        _apiKey = settings.ApiKey ?? throw new ArgumentNullException(nameof(settings.ApiKey));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }
public async Task<string> CreateChatCompletionAsync(string systemMessage, string userMessage)
{
    var requestBody = new
    {
        model = "gpt-4", // Use the latest model available to you.
        messages = new[]
        {
            new { role = "system", content = systemMessage },
            new { role = "user", content = userMessage }
        }
    };

    string jsonContent = JsonConvert.SerializeObject(requestBody);
    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

    HttpResponseMessage response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

    if (response.IsSuccessStatusCode)
    {
        string responseContent = await response.Content.ReadAsStringAsync();
        return responseContent;
    }
    else
    {
        string errorContent = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Error calling OpenAI API: {errorContent}");
    }
}

}
