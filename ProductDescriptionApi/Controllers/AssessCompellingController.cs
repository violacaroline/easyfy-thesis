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
public class AssessCompellingController : ControllerBase
{
    private readonly string _inputFilePath;
    private readonly string _resultsFilePath;
    private readonly string _resultsConfusionMatrixFilePath;
    private readonly int _totalIterations;
    private readonly string? _GptModel;
    private readonly IConfiguration _configuration;
    private readonly OpenAIService _openAIApiService;
    private readonly CsvHandler _csvHandler;

    public AssessCompellingController(IConfiguration configuration, OpenAIService openAIApiService, CsvHandler csvHandler)
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
        double truePositive = 0;
        double trueNegative = 0;
        double totalProductDescriptions;
        string assessmentType= "Compelling";


        _csvHandler.InitializeCsvWithHeaders(_resultsFilePath, _totalIterations);

        var batchResults = new Dictionary<int, List<int>>();
        List<ProductDescription> descriptions = _csvHandler.ReadDescriptionsAndAttributesFromCSV(_inputFilePath);
        totalProductDescriptions = descriptions.Count;
        for (int productNumber = 0; productNumber < descriptions.Count; productNumber++)
        {
            batchResults.Add(productNumber + 1, new List<int>());
        }

        for (int iterationNumber = 0; iterationNumber < _totalIterations; iterationNumber++)
        {
            for (int productNumber = 0; productNumber < descriptions.Count; productNumber++)
            {
                var response = await AssessDescriptionAsync(descriptions[productNumber]);
                if (response == null)
                {
                    batchResults[productNumber + 1].Add(-1);
                    continue;
                }

                string GroundTruth = descriptions[productNumber].Correctness;

                var messageContent = ParseApiResponse(response);

                Console.WriteLine("-----------------------------");
                Console.WriteLine("PRODUCTnr: ");
                Console.WriteLine(productNumber + 1);
                Console.WriteLine("GroundTruth: ");
                Console.WriteLine(descriptions[productNumber].Correctness);
                Console.WriteLine("Chat gpt :");
                Console.WriteLine(messageContent);
                Console.WriteLine("-----------------------------");

                if (messageContent.Contains("correct", StringComparison.OrdinalIgnoreCase) && GroundTruth == "Correct")
                {
                    Console.WriteLine(1);
                    truePositive++;
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
                    trueNegative++;
                    batchResults[productNumber + 1].Add(1);
                }
                else if (messageContent.Contains("wrong", StringComparison.OrdinalIgnoreCase) && GroundTruth == "Correct")
                {
                    Console.WriteLine(4);
                    batchResults[productNumber + 1].Add(0);
                }
            }
        }

        double accuracy = (truePositive + trueNegative) / totalProductDescriptions;
        Console.WriteLine($"truePositive = {truePositive}, trueNegative: {trueNegative},  totalProductDescriptions: {totalProductDescriptions} = {accuracy}" );
        // Write all results to CSV at once
        _csvHandler.WriteResultsToCSV(batchResults, _resultsFilePath);
        _csvHandler.WriteConfusionMatrixResultsToCSV(accuracy, _resultsConfusionMatrixFilePath, assessmentType,   _GptModel);
        return Ok();
    }


    private async Task<string> AssessDescriptionAsync(ProductDescription productInfo)
    {
        // string systemMessage = "Assess if the following text contains any spelling, grammatical, or punctuation errors. Only return 'Wrong' if the text needs corrections, and 'correct' if the text is correct.";
        // double temperature = 1;
        // try
        // {
        //     return await _openAIApiService.CreateChatCompletionAsync(systemMessage, productInfo.Description, temperature);
        // }
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
