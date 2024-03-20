using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProductDescriptionApi.Models;
using ProductDescriptionApi.Services;
using System.Threading.Tasks;

namespace ProductDescriptionApi.Controllers;

[ApiController]
[Route("language-assess")]
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
    var descriptions = _csvHandler.ReadDescriptionsFromCSV("assessment_data/assessment_input/to_assess_language_description.csv");
    Console.WriteLine("description");
    foreach (var description in descriptions)
    {
      Console.WriteLine(description);
    }
    return Ok(response);
  }


  [HttpPost("assess")]
  public async Task<IActionResult> AssessDescriptions()
  {
    {
      var filePath = "assessment_data/assessment_input/to_assess_language_descriptions.csv";
      var descriptions = ReadDescriptions(filePath);

      for (int i = 0; i < descriptions.Count; i++)
      {
        var response = await AssessDescriptionAsync(descriptions[i]);
        if (response == null) continue; // Decide how to handle null responses.

        var messageContent = ParseApiResponse(response);
        Console.WriteLine(messageContent);
        if ($"{messageContent.ToLower()}" == "correct")
        {
          WriteAssessedDescriptionToCSV("Correct", "assessment_data/assessment_results/assessed_language_results.csv");
        }
        else
        {
          WriteAssessedDescriptionToCSV("Wrong", "assessment_data/assessment_results/assessed_language_results.csv");
        }
      }

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
    string systemMessage = "Följande text kan ha stavfel, grammatiska fel eller fel med skiljetecken. Returnera endast \"Incorrect\" om texten behöver rättas utan kommentarer eller andra tillägg. Skriv  endast \"correct\"  om texten är korrekt.";
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