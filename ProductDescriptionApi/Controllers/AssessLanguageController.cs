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
        _csvHandler.InitializeCsvWithHeaders(_resultsFilePath, _totalIterations);

        var batchResults = new Dictionary<int, List<int>>();
        List<ProductDescription> descriptions = _csvHandler.ReadDescriptionsAndAttributesFromCSV(_inputFilePath);
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

        // Write all results to CSV at once
        _csvHandler.WriteResultsToCSV(batchResults, _resultsFilePath);

        return Ok();
    }


    private async Task<string> AssessDescriptionAsync(ProductDescription productInfo)
    {
        string systemMessage = "Assess if the following text contains any spelling, grammatical, or punctuation errors. Only return 'Wrong' if the text needs corrections, and 'correct' if the text is correct.";
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
