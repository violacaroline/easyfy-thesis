using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProductDescriptionApi.Models;

namespace ProductDescriptionApi.Services
{
    public class PDEService
    {
        private readonly OpenAIService _openAIApiService;
        private readonly ILogger<PDEService> _logger;
        private readonly double _temperature;

        public PDEService(OpenAIService openAIApiService, ILogger<PDEService> logger)
        {
            _openAIApiService = openAIApiService;
            _logger = logger;
        }

        public async Task<string> AssessDescriptionAsync(string assessingPrompt, string productDescription)
        {
            string systemMessage = assessingPrompt;
            string userMessage = productDescription;

            try
            {
                var response = await _openAIApiService.CreateChatCompletionAsync(systemMessage, userMessage, _temperature);
                string messageContent = ParseApiResponse(response);
                return EvaluatePD(messageContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling the OpenAI service.");
                return "Error calling the OpenAI service.";
            }
        }

        private string ParseApiResponse(string response)
        {
            try
            {
                var parsedResponse = JsonSerializer.Deserialize<ApiResponse>(response);
                return parsedResponse?.choices?[0]?.message?.content ?? "No response";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing response.");
                return "Error parsing response.";
            }
        }

        private string EvaluatePD(string messageContent)
        {
            if (messageContent.Contains("correct", StringComparison.OrdinalIgnoreCase))
            {
                return "Correct";
            }
            else if (messageContent.Contains("wrong", StringComparison.OrdinalIgnoreCase))
            {
                return "Wrong";
            }
            return "Unknown";
        }
    }
}
