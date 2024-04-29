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
        double truePositive = 0;
        double trueNegative = 0;
        double falsePositive = 0;
        double falseNegative = 0;
        double totalProductDescriptions;
        string assessmentType = "Language";
        _csvHandler.InitializeCsvWithHeaders(_resultsFilePath, _totalIterations);

        var batchResultsDetails = new Dictionary<int, List<int>>();
        List<ProductDescription> descriptions = _csvHandler.ReadDescriptionsAndAttributesFromCSV(_inputFilePath);
        totalProductDescriptions = descriptions.Count;
        for (int productNumber = 0; productNumber < descriptions.Count; productNumber++)
        {
            batchResultsDetails.Add(productNumber + 1, new List<int> { 0, 0, 0, 0 });
        }

        for (int iterationNumber = 0; iterationNumber < _totalIterations; iterationNumber++)
        {
            for (int productNumber = 0; productNumber < descriptions.Count; productNumber++)
            {
                var response = await AssessDescriptionAsync(descriptions[productNumber]);
                if (response == null)
                {
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
                    batchResultsDetails[productNumber + 1][0]++;
                }
                else if (messageContent.Contains("correct", StringComparison.OrdinalIgnoreCase) && GroundTruth == "Wrong")
                {
                    Console.WriteLine(2);
                    falsePositive++;
                    batchResultsDetails[productNumber + 1][1]++;
                }
                else if (messageContent.Contains("wrong", StringComparison.OrdinalIgnoreCase) && GroundTruth == "Wrong")
                {
                    Console.WriteLine(3);
                    trueNegative++;
                    batchResultsDetails[productNumber + 1][2]++;
                }
                else if (messageContent.Contains("wrong", StringComparison.OrdinalIgnoreCase) && GroundTruth == "Correct")
                {
                    Console.WriteLine(4);
                    falseNegative++;
                    batchResultsDetails[productNumber + 1][3]++;
                }
            }
        }
        double accuracy = (truePositive + trueNegative) / (totalProductDescriptions * _totalIterations);
       Console.WriteLine($"truePositive = {truePositive}, trueNegative: {trueNegative}, falsePositive: {falsePositive}, falseNegative: {falseNegative},  totalProductDescriptions: {totalProductDescriptions * _totalIterations} = {accuracy}");
        _csvHandler.WriteConfusionMatrixResultsToCSV(accuracy, _resultsConfusionMatrixFilePath, assessmentType, _GptModel);

        // Write all results to CSV at once
        _csvHandler.WriteConfusionMatrixDetailsToCSV(batchResultsDetails, _resultsFilePath);

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
