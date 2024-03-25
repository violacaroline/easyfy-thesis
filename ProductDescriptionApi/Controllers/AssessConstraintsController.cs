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
      int totalIterations = 1;
      _csvHandler.InitializeCsvWithHeaders(_resultsFilePath, totalIterations);

      // Store results for batch writing: Product Number => List of Results ("Correct" or "Wrong")
      var batchResults = new Dictionary<int, List<int>>();
      //   var descriptions = ReadDescriptions(filePath);
      List<ProductDescription> descriptionsAndAttributes = _csvHandler.ReadDescriptionsAndAttributesFromCSV(_inputFilePath);
      // Prepare the dictionary to hold results for each product
      for (int productNumber = 0; productNumber < descriptionsAndAttributes.Count; productNumber++)
      {
        batchResults.Add(productNumber + 1, new List<int>());
      }
      // Assess descriptions across iterations
      for (int iterationNumber = 0; iterationNumber < totalIterations; iterationNumber++)
      {
        for (int productNumber = 0; productNumber < descriptionsAndAttributes.Count; productNumber++)
        {
          var response = await AssessDescriptionAsync(descriptionsAndAttributes[productNumber]);
          if (response == null)
          {
            // Decide how to handle null responses. Here, adding "Error" to indicate a failed assessment.
            batchResults[productNumber + 1].Add(-1);
            continue;
          }
          string GroundTruth = descriptionsAndAttributes[productNumber].Correctness;
          var messageContent = ParseApiResponse(response);
          Console.WriteLine("-----------------------------");
          Console.WriteLine("PRODUCTnr: ");
          Console.WriteLine(productNumber +1);
          Console.WriteLine("GroundTruth: ");
          Console.WriteLine(descriptionsAndAttributes[productNumber].Correctness);
          Console.WriteLine("Chat gpt :");
          Console.WriteLine(messageContent);
          Console.WriteLine("-----------------------------");
          if (messageContent.Contains("correct", StringComparison.OrdinalIgnoreCase) && GroundTruth == "Correct")
          {
            Console.WriteLine(1);
            batchResults[productNumber + 1].Add(1);
          }
          else if (messageContent.Contains("correct", StringComparison.OrdinalIgnoreCase) && GroundTruth == "Wrong")
          {
            Console.WriteLine(2);
            batchResults[productNumber + 1].Add(0);
          }
          else if (messageContent.Contains("wrong", StringComparison.OrdinalIgnoreCase) && GroundTruth == "Wrong")
          {
            Console.WriteLine(3);
            batchResults[productNumber + 1].Add(1);
          }
          else if (messageContent.Contains("wrong", StringComparison.OrdinalIgnoreCase) && GroundTruth == "Correct")
          {
            Console.WriteLine(4);
            batchResults[productNumber + 1].Add(0);
          }
        }
      }

      // Write all results to CSV at once, now that assessments are complete
      WriteAssessedDescriptionToCSV(batchResults);

      return Ok();
    }
  }


  private async Task<string> AssessDescriptionAsync(ProductDescription productInfo)
  {
    string systemMessage = $"Kontrollera om följande text innehåller alla dessa punkter: [{productInfo.Attributes}].Returnera endast \'correct\' om texten innehåller allt som finns i listan utan att ge några ytterligare kommentarer eller detaljer, returnera endast \"wrong\" om texten saknar en eller flera av det som finns i listan utan att ge några ytterligare kommentarer eller detaljer.";
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

  private void WriteAssessedDescriptionToCSV(Dictionary<int, List<int>> batchResults)
  {
    try
    {
      _csvHandler.WriteResultsToCSV(batchResults, _resultsFilePath);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error writing to CSV: {ex.Message}");
      throw;
    }
  }

}
