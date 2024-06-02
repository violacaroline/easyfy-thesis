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


        public async Task<string> AssessDescriptionAsync(ProductDescription productInfo, string assessingPrompt)
        {
            string systemMessage = assessingPrompt;
            
            try
            {
                var response = await _openAIApiService.CreateChatCompletionAsync(systemMessage, productInfo.Description, Temperature);
                return ParseApiResponse(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling the OpenAI service: {ex.Message}");
                return null;
            }
        }

        public async Task<string> AssessDescriptionAsync(ProductDescription productInfo, string assessingPrompt, string constraintPD)
        {
            string systemMessage = assessingPrompt;
            try
            {

                string prompt = $"Text: {productInfo.Description}";
                Console.WriteLine($"Prompt: {prompt}");

               var response = await _openAIApiService.CreateChatCompletionAsync(systemMessage, prompt, Temperature);
                return ParseApiResponse(response);
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