using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProductDescriptionApi.Models;
using ProductDescriptionApi.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductDescriptionApi.Controllers;

[ApiController]
[Route("language-assessment")]
public class AssessLanguageController : ControllerBase
{
  private readonly string _inputFilePath;
  private readonly string _resultsFilePath;
  private readonly string _resultsConfusionMatrixFilePath;
  private readonly string? _GptModel;
  private readonly int _totalIterations;
  private readonly IConfiguration _configuration;
  private readonly OpenAIService _openAIApiService;
  private readonly CsvHandler _csvHandler;

  public AssessLanguageController(IConfiguration configuration, OpenAIService openAIApiService, CsvHandler csvHandler)
  {
    _configuration = configuration;
    _openAIApiService = openAIApiService;
    _csvHandler = csvHandler;
    _inputFilePath = _configuration["InputFilePath:Language"];
    _resultsFilePath = _configuration["ResultsFilePath:Language"];
    _resultsConfusionMatrixFilePath = _configuration["ResultsFilePath:Results"];
    _GptModel = _configuration["GptModel"];
    _totalIterations = _configuration.GetValue<int>("TotalIterations");
  }

  [HttpGet]
  public IActionResult Get()
  {
    var response = new { Message = "Hello! Here you can assess your texts for language correctness." };
    return Ok(response);
  }

  [HttpPost("assess")]
  public async Task<IActionResult> AssessDescriptions()
  {
    string assessmentType = "Language";

    var batchResultsDetails = new List<Tuple<int, string, string>>();
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
        Console.WriteLine("The product description is linguistically correct");
        batchResultsDetails.Add(Tuple.Create(productNumber + 1, descriptions[productNumber].Description, "correct"));
      }

      else if (messageContent.Contains("wrong", StringComparison.OrdinalIgnoreCase))
      {
        Console.WriteLine("The product description can contain one or more language error");
        batchResultsDetails.Add(Tuple.Create(productNumber + 1, descriptions[productNumber].Description, "wrong"));
      }
    }

    _csvHandler.WriteAssessmentsResultsToCSV(batchResultsDetails, _resultsFilePath);
    return Ok();
  }


  private async Task<string> AssessDescriptionAsync(ProductDescription productInfo)
  {
    string systemMessage = "Bedöm om följande text innehåller några stavfel, grammatiska fel eller fel skiljetecken. Säkerställ att possessiva pronomenen passar substantiven. Returnera endast 'Wrong' om texten behöver korrigeringar, och 'Correct' om texten är korrekt. utan att ge några ytterligare kommentarer eller detaljer.";
    double temperature = 1;
    try
    {
      return await _openAIApiService.CreateChatCompletionAsync(systemMessage, productInfo.Description, temperature);
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