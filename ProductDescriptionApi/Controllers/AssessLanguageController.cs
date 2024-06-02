using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using ProductDescriptionApi.Models;
using ProductDescriptionApi.Services;

namespace ProductDescriptionApi.Controllers
{
    [ApiController]
    [Route("language-assessment")]
    public class AssessLanguageController : ControllerBase
    {
        private readonly OpenAIService _openAIApiService;

        public AssessLanguageController(OpenAIService openAIApiService)
        {
            _openAIApiService = openAIApiService;
        }

        [HttpPost("assess")]
        public async Task<IActionResult> AssessDescriptions([FromBody] ProductDescription request)
        {
            var response = await AssessDescriptionAsync(request);
            var messageContent = ParseApiResponse(response);

            Console.WriteLine("-----------------------------");
            Console.WriteLine("Chat gpt :");
            Console.WriteLine(messageContent);
            Console.WriteLine("-----------------------------");

            if (messageContent.Contains("correct", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("The product description is linguistically correct");
                return Ok("Correct");
            }
            else if (messageContent.Contains("wrong", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("The product description can contain one or more language error");
                return Ok("Wrong");
            }

            return Ok();
        }

        private async Task<string> AssessDescriptionAsync(ProductDescription productDescription)
        {
            string systemMessage = "Bedöm om följande text innehåller några stavfel, grammatiska fel eller fel skiljetecken. Säkerställ att possessiva pronomenen passar substantiven. Returnera endast 'Wrong' om texten behöver korrigeringar, och 'Correct' om texten är korrekt. utan att ge några ytterligare kommentarer eller detaljer.";
            double temperature = 1;
            try
            {
                return await _openAIApiService.CreateChatCompletionAsync(systemMessage, productDescription.Description, temperature);
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