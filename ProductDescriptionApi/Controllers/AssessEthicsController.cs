using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProductDescriptionApi.Models;
using ProductDescriptionApi.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductDescriptionApi.Controllers;

[ApiController]
[Route("compelling-assessment")]
public class AssessEthicsController : ControllerBase
{
  private readonly string _inputFilePath;
  private readonly string _resultsFilePath;
  private readonly string _resultsConfusionMatrixFilePath;
  private readonly int _totalIterations;
  private readonly string? _GptModel;
  private readonly IConfiguration _configuration;
  private readonly OpenAIService _openAIApiService;
  private readonly CsvHandler _csvHandler;

  public AssessEthicsController(IConfiguration configuration, OpenAIService openAIApiService, CsvHandler csvHandler)
  {
    _configuration = configuration;
    _openAIApiService = openAIApiService;
    _csvHandler = csvHandler;
    _inputFilePath = _configuration["InputFilePath:Compelling"];
    _resultsFilePath = _configuration["ResultsFilePath:Compelling"];
    _resultsConfusionMatrixFilePath = _configuration["ResultsFilePath:Results"];
    _totalIterations = _configuration.GetValue<int>("TotalIterations");
    _GptModel = _configuration["GptModel"];
  }

  [HttpGet]
  public IActionResult Get()
  {
    var response = new { Message = "Hello! Here you can assess your texts for Compelling correctness." };
    return Ok(response);
  }

  [HttpPost("assess")]
  public async Task<IActionResult> AssessDescriptions()
  {


    string assessmentType = "Compelling";


    var batchResultsDetails = new List<Tuple<int, string, string>>();
    // Read descriptions from CSV
    List<ProductDescription> descriptions = _csvHandler.ReadDescriptionsAndAttributesFromCSV(_inputFilePath);

    for (int productNumber = 0; productNumber < descriptions.Count; productNumber++)
    {
      var response = await AssessDescriptionAsync(descriptions[productNumber]);
      if (response == null)
      {
        batchResultsDetails.Add(Tuple.Create(productNumber + 1, descriptions[productNumber].Description, "Error"));
        continue;
      }

      var messageContent = ParseApiResponse(response);

      Console.WriteLine("-----------------------------");
      Console.WriteLine("PRODUCTnr: ");
      Console.WriteLine(productNumber + 1);
      Console.WriteLine("Chat gpt :");
      Console.WriteLine(messageContent);
      Console.WriteLine("-----------------------------");

      if (messageContent.Contains("correct", StringComparison.OrdinalIgnoreCase))
      {
        Console.WriteLine("The product description comply to ethics");
        batchResultsDetails.Add(Tuple.Create(productNumber + 1, descriptions[productNumber].Description, "correct"));
      }

      else if (messageContent.Contains("wrong", StringComparison.OrdinalIgnoreCase))
      {
        Console.WriteLine("The product description can contain an ethical error");
        batchResultsDetails.Add(Tuple.Create(productNumber + 1, descriptions[productNumber].Description, "wrong"));
      }
    }

    _csvHandler.WriteAssessmentsResultsToCSV(batchResultsDetails, _resultsFilePath);
    return Ok();
  }


  private async Task<string> AssessDescriptionAsync(ProductDescription productInfo)
  {
    string systemMessage = $"Kontrollera om följande produktbeskrivning är övertygande eller inte övertygande, ta hänsyn till etiska aspekter gällande marknadsföring. Returnera endast \'correct\' om du anser att texten är övertygande utan att ge några ytterligare kommentarer eller detaljer. Returnera endast \"wrong\" om du anser att texten inte är övertygande utan att ge några ytterligare kommentarer eller detaljer. Jag är intresserad av att förstå om texten effektivt lockar kunden och framhäver produkten på ett positivt sätt. Tack!";
    double temperature = 1;
    try
    {

      string prompt = $"Produktbeskrivningen: {productInfo.Description}.";
      Console.WriteLine($"Prompt: {prompt}");

      return await _openAIApiService.CreateChatCompletionAsync(systemMessage, prompt, temperature);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error calling the OpenAI service: {ex.Message}");
      return null;
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
      return "Error parsing response";
    }
  }
}