using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProductDescriptionApi.Models;
using ProductDescriptionApi.Services;
using System.Threading.Tasks;

namespace ProductDescriptionApi.Controllers;

[ApiController]
[Route("product-description")]
public class ProductDescriptionController : ControllerBase
{
    private readonly OpenAIService _openAIApiService;

    // Constructor injection of OpenAIApiService
    public ProductDescriptionController( OpenAIService openAIApiService)
    {
        _openAIApiService = openAIApiService;
    }


    [HttpPost("generate")]
    public async Task<IActionResult> GenerateDescription([FromBody] ProductDescriptionRequest request)
    {

        string systemMessage = "You will be provided with a product name delimited by triple underscores and seed words that you will use to generate the description of the product. The description should follow the following constraints: it should be 5 sentences long, it should be compelling, it should be written in Swedish";
        // Ensure that the request parameters are not null
        string userMessage = request.productAttributes ?? "___Produkt Beskrivning Generator___, felfria, AI-teknologi, professionell, lockar kunder, hålla sig till marknadsföringsetik, Integrerar alla relevanta produktbeskrivningsnyckelord, maximal synlighet och SEO-effektivitet.";
        string temp = request.Temperature ?? "1.0";

        double Temperature = double.Parse(temp);

        // Call the OpenAI service with the system message and the user message
        string response = await _openAIApiService.CreateChatCompletionAsync(systemMessage, userMessage, Temperature);

        // Parse the JSON to extract message.content
        var parsedResponse = JsonConvert.DeserializeObject<ApiResponse>(response);
        string messageContent = parsedResponse?.choices?[0]?.message?.content ?? "No response";

        // Return only the message content
        return Ok(messageContent);
    }
}