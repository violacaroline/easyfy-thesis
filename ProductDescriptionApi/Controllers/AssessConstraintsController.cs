using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProductDescriptionApi.Models;
using ProductDescriptionApi.Services;
namespace ProductDescriptionApi.Controllers;

[ApiController]
[Route("constraints-assessment")]

public class AssessConstraintsController : ControllerBase

{
  private readonly string _inputFilePath;
  private readonly string _resultsFilePath;
  private readonly string _resultsConfusionMatrixFilePath;
  private readonly string? _GptModel;
  private readonly int _totalIterations;
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
    _resultsConfusionMatrixFilePath = _configuration["ResultsFilePath:Results"];
    _GptModel = _configuration["GptModel"];
    _totalIterations = _configuration.GetValue<int>("TotalIterations");
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

    // Read descriptions from CSV
    List<ProductDescription> descriptionsAndAttributes = _csvHandler.ReadDescriptionsAndAttributesFromCSV(_inputFilePath);

    // List to store results for batch writing: (Product Number, Product Description, Evaluation)
    var batchResultsDetails = new List<Tuple<int, string, string>>();

    // Assess each description
    for (int productNumber = 0; productNumber < descriptionsAndAttributes.Count; productNumber++)
    {
      var response = await AssessDescriptionAsync(descriptionsAndAttributes[productNumber]);
      if (response == null)
      {
        // Handle null responses. Here, adding "Error" to indicate a failed assessment.
        batchResultsDetails.Add(Tuple.Create(productNumber + 1, descriptionsAndAttributes[productNumber].Description, "Error"));
        continue;
      }
      var messageContent = ParseApiResponse(response);

      if (messageContent.Contains("correct", StringComparison.OrdinalIgnoreCase))
      {
        Console.WriteLine("The product description adheres to the constraints");
        batchResultsDetails.Add(Tuple.Create(productNumber + 1, descriptionsAndAttributes[productNumber].Description, "correct"));
      }
      else if (messageContent.Contains("wrong", StringComparison.OrdinalIgnoreCase))
      {
        Console.WriteLine("The product description contains constraint errors");
        batchResultsDetails.Add(Tuple.Create(productNumber + 1, descriptionsAndAttributes[productNumber].Description, "wrong"));
      }
    }

    // Write all results to CSV at once, now that assessments are complete
    _csvHandler.WriteAssessmentsResultsToCSV(batchResultsDetails, _resultsFilePath);

    return Ok();
  }

  [NonAction]
  public async Task<string> AssessSingleDescription(ProductDescription productDescription)
  {
    var response = await AssessDescriptionAsync(productDescription);
    return response == null ? "Error" : ParseApiResponse(response);
  }

  private async Task<string> AssessDescriptionAsync(ProductDescription productInfo)
  {
    string systemMessage = $"Vänligen verifiera att den tillhandahållna texten innehåller alla de angivna orden, fraserna och formuleringarna som listas i [{productInfo.Attributes}], eller deras synonymer och andra formuleringar som förmedlar samma betydelse. Svara med 'correct' om texten innehåller motsvarigheter för varje punkt på listan, antingen som specificerat eller genom godtagbara alternativ. Svara med 'wrong' om någon motsvarighet saknas eller inte adekvat förmedlar samma betydelse. Undvik att ge några ytterligare kommentarer eller detaljer.";
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

}
