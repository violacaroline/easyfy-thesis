using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProductDescriptionApi.Models;
using ProductDescriptionApi.Services;
namespace ProductDescriptionApi.Controllers;

[ApiController]
[Route("constraints-assess")]

public class AssessConstraintsController : ControllerBase

{
 private readonly OpenAIService _openAIApiService;
  private readonly CsvHandler _csvHandler;

  // Constructor injection of OpenAIApiService
  public AssessConstraintsController(OpenAIService openAIApiService, CsvHandler csvHandler)
  {
    _openAIApiService = openAIApiService;
    _csvHandler = csvHandler;
  }

  [HttpGet]
  public IActionResult Get()
  {
    var response = new { Message = "Hello! here can you assess your texts constraints" };
    return Ok(response);
  }

  [HttpPost("assess")]
  public async Task<IActionResult> AssessDescriptions()
  {
    {
      var filePath = "to_assess_constraints_descriptions.csv";
    //   var descriptions = ReadDescriptions(filePath);
    _csvHandler.ReadDescriptionsAndAttributesFromCSV(filePath);

    //   for (int i = 0; i < descriptions.Count; i++)
    //   {
    //     var response = await AssessDescriptionAsync(descriptions[i]);
    //     if (response == null) continue; // Decide how to handle null responses.

    //     var messageContent = ParseApiResponse(response);
    //     Console.WriteLine(messageContent);
    //     if ($"{messageContent.ToLower()}" == "correct")
    //     {
    //       WriteAssessedDescriptionToCSV("Correct", "assessed_description.csv");
    //     }
    //     else
    //     {
    //       WriteAssessedDescriptionToCSV("Wrong", "assessed_description.csv");
    //     }
    //   }

      return Ok();
    }

  }

  private List<string> ReadDescriptions(string filePath)
  {
    try
    {
      return _csvHandler.ReadDescriptionsFromCSV(filePath);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error reading descriptions from CSV: {ex.Message}");
      throw;
    }
  }

  private async Task<string> AssessDescriptionAsync(string description)
  {
    string systemMessage = "Jag skulle vilja veta om texten är skriven på svenska. Sedan, innehåller texten specifika ord och fraser. Skulle du kunna kontrollera texten punkt för punkt mot följande lista? Returnera endast \"correct\" när texten innehåller allt, och skriv vilka som saknas utan kommentarer eller andra tillägg.";
    double temperature = 1;
    try
    {
      return await _openAIApiService.CreateChatCompletionAsync(systemMessage, description, temperature);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error calling the OpenAI service: {ex.Message}");
      return null; // Return null or handle differently based on your error handling strategy.
    }
  }


  private string ParseApiResponse(string response)
  {
    try
    {
      var parsedResponse = JsonConvert.DeserializeObject<ApiResponse>(response);
      return parsedResponse?.choices?[0]?.message?.content ?? "No response";
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error parsing response: {ex.Message}");
      return "Error parsing response"; // Consider how you want to handle parse errors.
    }
  }

  private void WriteAssessedDescriptionToCSV(string messageContent, string filePath)
  {
    try
    {
      _csvHandler.WriteToCSV(messageContent, filePath);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error writing to CSV: {ex.Message}");
      throw;
    }
  }

}
