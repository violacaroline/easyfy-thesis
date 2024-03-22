using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProductDescriptionApi.Models;
using ProductDescriptionApi.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductDescriptionApi.Controllers;

[ApiController]
[Route("language-assess")]
public class AssessLanguageController : ControllerBase
{
    private readonly string _inputFilePath;
    private readonly string _resultsFilePath;
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
        int totalIterations = 2;
        _csvHandler.InitializeCsvWithHeaders(_resultsFilePath, totalIterations);
        
        var descriptions = ReadDescriptions(_inputFilePath);
        var batchResults = new Dictionary<int, List<int>>();

        for (int i = 0; i < descriptions.Count; i++)
        {
            batchResults.Add(i + 1, new List<int>()); // Initialize results list for each product
            for (int iteration = 0; iteration < totalIterations; iteration++)
            {
                var response = await AssessDescriptionAsync(descriptions[i]);
                if (response == null)
                {
                    batchResults[i + 1].Add(-1); // Handle null response
                    continue;
                }

                var messageContent = ParseApiResponse(response);
                if (messageContent.Contains("correct", StringComparison.OrdinalIgnoreCase))
                {
                    batchResults[i + 1].Add(1);
                }
                else
                {
                    batchResults[i + 1].Add(0);
                }
            }
        }

        // Write all results to CSV at once
        _csvHandler.WriteResultsToCSV(batchResults, _resultsFilePath);

        return Ok();
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
        string systemMessage = "Assess if the following text contains any spelling, grammatical, or punctuation errors. Only return 'Incorrect' if the text needs corrections, and 'correct' if the text is correct.";
        double temperature = 1;
        try
        {
            return await _openAIApiService.CreateChatCompletionAsync(systemMessage, description, temperature);
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
