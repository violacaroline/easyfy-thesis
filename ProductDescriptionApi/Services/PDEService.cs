using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using ProductDescriptionApi.Models;
using System.Data;

namespace ProductDescriptionApi.Services
{
    public class PDEService
    {
        private readonly OpenAIService _openAIApiService;
        private double Temperature = 1;

        public PDEService(OpenAIService openAIApiService)
        {
            _openAIApiService = openAIApiService;
        }


        public async Task<string> AssessDescriptionAsync(string assessingPrompt, string productDescription)
        {
            string systemMessage = assessingPrompt;
            string userMessage = productDescription;

            try
            {
                var response = await _openAIApiService.CreateChatCompletionAsync(systemMessage, userMessage, Temperature);
                return ParseApiResponse(response);
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
                var parsedResponse = JsonSerializer.Deserialize<ApiResponse>(response);
                return parsedResponse?.choices?[0]?.message?.content ?? "No response";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing response: {ex.Message}");
                return "Error parsing response";
            }
        }

    }
}