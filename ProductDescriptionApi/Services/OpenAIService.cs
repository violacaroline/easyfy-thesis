namespace ProductDescriptionApi.Services;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
public class OpenAIService
{
  private readonly HttpClient _httpClient;
  private readonly string _apiKey;
  public OpenAIService(HttpClient httpClient, IConfiguration configuration)
  {
    _httpClient = httpClient;
    _apiKey = configuration["OpenAI:ApiKey"];
    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
  }
  public async Task<string> GenerateResponseAsync(string prompt)
  {
    var data = new
    {
      model = "text-davinci-003",
      prompt = prompt,
      temperature = 0.7,
      max_tokens = 150
    };
    var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/engines/davinci/completions", data);
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<dynamic>();
    return result.choices[0].text;
  }
}