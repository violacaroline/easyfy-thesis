using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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


    [HttpPost("generate")]
public async Task<IActionResult> GenerateDescription([FromBody] ProductDescriptionRequest request)
{
    // Ensure that the request parameters are not null
    string systemMessage = request.SystemMessage ?? "string";
    string userMessage = request.UserMessage ?? "Default User Message";

    // Call the OpenAI service with the system message and the user message
    string response = await _openAIApiService.CreateChatCompletionAsync(systemMessage, userMessage);

    // Assuming response is a JSON string that contains the message.content field
    // Parse the JSON to extract message.content
    var parsedResponse = JsonConvert.DeserializeObject<ApiResponse>(response);
    string messageContent = parsedResponse?.choices?[0]?.message?.content ?? "No response";

    // Return only the message content
    return Ok(messageContent);
}

// Define the ApiResponse class according to the expected structure of the response
public class ApiResponse
{
    public List<Choice>? choices { get; set; }
}

public class Choice
{
    public Message? message { get; set; }
}

public class Message
{
    public string? content { get; set; }
}

}

    public class ProductDescriptionRequest
    {
        public string? SystemMessage { get; set; }
        public string? UserMessage { get; set; }
    }