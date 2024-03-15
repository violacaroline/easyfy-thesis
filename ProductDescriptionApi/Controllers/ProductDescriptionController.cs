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
        string Temp = request.Temperature ?? "0.7";

        double Temperature = double.Parse(Temp);
        // Call the OpenAI service with the system message and the user message
        string response = await _openAIApiService.CreateChatCompletionAsync(systemMessage, userMessage, Temperature);

        // Assuming response is a JSON string that contains the message.content field
        // Parse the JSON to extract message.content
        var parsedResponse = JsonConvert.DeserializeObject<ApiResponse>(response);
        string messageContent = parsedResponse?.choices?[0]?.message?.content ?? "No response";

        
        // Write the response to a CSV file
        WriteToCSV(messageContent, "generated_description.csv");

        // Return only the message content
        return Ok(messageContent);
    }

    private void WriteToCSV(string text, string filePath)
    {
        // Write the text to a CSV file (append mode)
        using var writer = new StreamWriter(filePath, append: true);
        writer.WriteLine(text);
    }
}
