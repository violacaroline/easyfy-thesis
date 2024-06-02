using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;


public class OpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _gptModel;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAIService> _logger;
    public OpenAIService(HttpClient httpClient, IOptions<OpenAIServiceOptions> options, IConfiguration configuration, ILogger<OpenAIService> logger)
    {
        _configuration = configuration;
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        var settings = options.Value ?? throw new ArgumentNullException(nameof(options));
        _apiKey = settings.ApiKey ?? throw new ArgumentNullException(nameof(settings.ApiKey));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        _gptModel = _configuration["GptModel"] ?? throw new ArgumentNullException("GptModel configuration is missing");
    }
    public async Task<string> CreateChatCompletionAsync(string systemMessage, string userMessage, double temperature)
    {
        var requestBody = new
        {
            model = _gptModel,
            messages = new[]
            {
            new { role = "system", content = systemMessage },
            new { role = "user", content = userMessage }
        },
            temperature = temperature,
            n = 1
        };

        Console.WriteLine($"----------------------------------------------");
        Console.WriteLine($"request.temperature:{requestBody.temperature}");
        Console.WriteLine($"----------------------------------------------");

        string jsonContent = JsonConvert.SerializeObject(requestBody);

        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        try
        {
            _logger.LogInformation("Sending request to OpenAI API.");
            HttpResponseMessage response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                return responseContent;
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error calling OpenAI API: {response.ReasonPhrase} - {errorContent}");
                throw new HttpRequestException($"Error calling OpenAI API: {response.ReasonPhrase} - {errorContent}");
            }
        }

        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP request error occurred while calling OpenAI API.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while calling OpenAI API.");
            throw;
        }
    }
}
