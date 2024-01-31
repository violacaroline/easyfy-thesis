using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ProductDescriptionApi.Controllers;

[ApiController]
[Route("product-description")]
public class ProductDescriptionController : ControllerBase
{
    private readonly OpenAIService _openAIApiService;

    // Constructor injection of OpenAIApiService
    public ProductDescriptionController(OpenAIService openAIApiService)
    {
        _openAIApiService = openAIApiService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var response = new { Message = "Hello! You are talking to the API that will generate product descriptions using AI" };
        return Ok(response);
    }
    // Endpoint to generate product description using chat
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateDescription([FromBody] ProductDescriptionRequest request)
    {
        var SystemMessage = "string";
        // // Call the OpenAI service with the system message and the new user message
        string response = await _openAIApiService.CreateChatCompletionAsync(SystemMessage, request.UserMessage);
        // Parse the response and return it
        return Ok(new { Response = response });
    }

}

public class ProductDescriptionRequest
{
    public string SystemMessage { get; set; }
    public string UserMessage { get; set; }
}