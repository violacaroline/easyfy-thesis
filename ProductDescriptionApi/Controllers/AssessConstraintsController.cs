using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProductDescriptionApi.Models;
using ProductDescriptionApi.Services;
namespace ProductDescriptionApi.Controllers;

[ApiController]
[Route("constraints-assess")]

public class AssessConstraintsController : ControllerBase

{
  private readonly string _inputFilePath;
  private readonly string _resultsFilePath;
  private readonly OpenAIService _openAIApiService;
  private readonly CsvHandler _csvHandler;
  private readonly IConfiguration _configuration;

  // Constructor injection of OpenAIApiService
  public AssessConstraintsController(IConfiguration configuration, OpenAIService openAIApiService, CsvHandler csvHandler)
  {
    _configuration = configuration;
    _openAIApiService = openAIApiService;
    _csvHandler = csvHandler;
    _inputFilePath = _configuration["InputFilePath:Constraints"];
    _resultsFilePath = _configuration["ResultsFilePath:Constraints"];
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
      
      //   var descriptions = ReadDescriptions(filePath);
      List<ProductDescription> descriptionsAndAttributes = _csvHandler.ReadDescriptionsAndAttributesFromCSV(_inputFilePath);

        for (int i = 0; i < descriptionsAndAttributes.Count; i++)
        {
          var response = await AssessDescriptionAsync(descriptionsAndAttributes[i]);
          if (response == null) continue; // Decide how to handle null responses.

          var messageContent = ParseApiResponse(response);
          if (messageContent.Contains("correct", StringComparison.OrdinalIgnoreCase))
          {
            WriteAssessedDescriptionToCSV("Correct", _resultsFilePath, i);
          }
          else
          {
            WriteAssessedDescriptionToCSV("Wrong", _resultsFilePath, i);
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

  private async Task<string> AssessDescriptionAsync(ProductDescription productInfo)
  {
    string systemMessage = $"Kontrollera om följande text innehåller alla dessa punkter: [{productInfo.Attributes}].Returnera endast \'correct\' om texten innehåller allt som finns i listan utan att ge några ytterligare kommentarer eller detaljer, returnera endast \"incorrect\" om texten saknar en eller flera av det som finns i listan utan att ge några ytterligare kommentarer eller detaljer.";
    double temperature = 1;
    try
    {

      string prompt = $"Text: {productInfo.Description}";
      Console.WriteLine($"Prompt: {prompt}");

      return await _openAIApiService.CreateChatCompletionAsync(systemMessage, prompt, temperature);
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

  private void WriteAssessedDescriptionToCSV(string messageContent, string filePath, int productNumber)
  {
    try
    {
      _csvHandler.WriteToCSV(messageContent, filePath, productNumber);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error writing to CSV: {ex.Message}");
      throw;
    }
  }

}
