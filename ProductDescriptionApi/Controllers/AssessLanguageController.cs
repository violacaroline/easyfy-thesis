using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProductDescriptionApi.Models;
using ProductDescriptionApi.Services;
using System.Threading.Tasks;

namespace ProductDescriptionApi.Controllers;

[ApiController]
[Route("language-asses")]
public class AssessLanguageController : ControllerBase
{
  private readonly OpenAIService _openAIApiService;
  private readonly CsvHandler _csvHandler;

  // Constructor injection of OpenAIApiService
  public AssessLanguageController(OpenAIService openAIApiService, CsvHandler csvHandler)
  {
    _openAIApiService = openAIApiService;
    _csvHandler = csvHandler;
  }

  [HttpGet]
  public IActionResult Get()
  {
    var response = new { Message = "Hello! here can you assess your texts" };
    var descriptions = _csvHandler.ReadDescriptionsFromCSV("generated_description.csv");
      Console.WriteLine("description");
    foreach (var description in descriptions)
    {
      Console.WriteLine(description);
    }
    return Ok(response);
  }


  [HttpPost("assess")]
  public async Task<IActionResult> GenerateDescription()
  {
    var descriptions = _csvHandler.ReadDescriptionsFromCSV("generated_description.csv");
    foreach (var description in descriptions)
    {
      Console.WriteLine(description);
          // Ensure that the request parameters are not null
    string systemMessage = "Följande text kan ha stavfel, grammatiska fel eller fel med skiljetecken. Returnera endast den rättade texten utan kommentarer eller andra tillägg. Behåll texten som den är om den är korrekt. Skriv \"correct\" när du behåller texten.";
    string userMessage = description;
    double Temperature = 0.7;

    // Call the OpenAI service with the system message and the user message
    string response = await _openAIApiService.CreateChatCompletionAsync(systemMessage, userMessage, Temperature);

    // Assuming response is a JSON string that contains the message.content field
    // Parse the JSON to extract message.content
    var parsedResponse = JsonConvert.DeserializeObject<ApiResponse>(response);
    string messageContent = parsedResponse?.choices?[0]?.message?.content ?? "No response";


    // Write the response to a CSV file
    _csvHandler.WriteToCSV(messageContent, "assessed_description.csv");
    }
    // Return only the message content
    return Ok();
  }

}