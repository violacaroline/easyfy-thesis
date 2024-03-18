using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class ProductDescriptionService
{
    private readonly HttpClient _httpClient;

    public ProductDescriptionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GenerateProductDescriptionAsync(string productName, string keywords, string systemMessage, string temperature)
    {
        try
        {
            var formattedTemperature = temperature.Replace(',', '.');

            Console.WriteLine($"----------------------------------------------");
            Console.WriteLine($"Formatted Temperature: {formattedTemperature}");
            Console.WriteLine($"SystemMessage: {systemMessage}");
            Console.WriteLine($"UserMessage: {$"\"\"\"{productName}\"\"\"\nseedWords: {keywords}"}");

            Console.WriteLine($"----------------------------------------------");
            var response = await _httpClient.PostAsJsonAsync("http://localhost:5000/product-description/generate", new
            {
                temperature = formattedTemperature,
                systemMessage = systemMessage,
                userMessage = $"\"\"\"{productName}\"\"\"{keywords}",
                attributes = keywords
            });

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return $"Error: {response.ReasonPhrase} - {errorContent}";
            }
        }
        catch (HttpRequestException ex)
        {
            return $"Request failed: {ex.Message}";
        }
    }
}
