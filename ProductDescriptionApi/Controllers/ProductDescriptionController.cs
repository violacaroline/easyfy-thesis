using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProductDescriptionApi.Models;
using System.Threading.Tasks;

namespace ProductDescriptionApi.Controllers;

[ApiController]
[Route("product-description")]
public class ProductDescriptionController : ControllerBase
{
    private readonly OpenAIService _openAIApiService;
    private readonly ILogger<ProductDescriptionController> _logger;

    public ProductDescriptionController(OpenAIService openAIApiService, ILogger<ProductDescriptionController> logger)
    {
        _openAIApiService = openAIApiService;
        _logger = logger;
    }


    [HttpPost("generate")]
    public async Task<IActionResult> GenerateDescription([FromBody] ProductDescriptionRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.productAttributes))
        {
            _logger.LogWarning("Invalid request payload.");
            return BadRequest("Invalid request payload.");
        }

        string systemMessage = "You will be provided with a product name delimited by triple underscores and seed words that you will use to generate the description of the product. The description should follow the following constraints: it should be 5 sentences long, it should be compelling, it should be written in Swedish";

        string userMessage = request.productAttributes ?? "___Produkt Beskrivning Generator___, felfria, AI-teknologi, professionell, lockar kunder, hålla sig till marknadsföringsetik, Integrerar alla relevanta produktbeskrivningsnyckelord, maximal synlighet och SEO-effektivitet.";

        string temp = request.Temperature ?? "1.0";

        double temperature = double.Parse(temp);

        try
        {
            string response = await _openAIApiService.CreateChatCompletionAsync(systemMessage, userMessage, temperature);

            var parsedResponse = JsonConvert.DeserializeObject<ApiResponse>(response);
            string messageContent = parsedResponse?.choices?[0]?.message?.content ?? "No response";

            return Ok(messageContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while generating product description.");
            return StatusCode(500, "Internal server error occurred while generating product description.");
        }
    }
}