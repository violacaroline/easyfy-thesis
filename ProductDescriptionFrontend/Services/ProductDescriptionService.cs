using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class ProductDescriptionService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;

    public ProductDescriptionService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiBaseUrl = configuration.GetValue<string>("APIBaseUrl") ?? throw new ArgumentNullException("APIBaseUrl");
    }

    public async Task<string> GenerateProductDescriptionAsync(string productName, string keywords)
    {
        try
        {
            // var formattedTemperature = temperature.Replace(',', '.');

            var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}/product-description/generate", new
            {
                productAttributes = $"___{productName}___{keywords}",
                temperature = "1.0"
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
