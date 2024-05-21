using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProductDescriptionApi.Models;
using ProductDescriptionApi.Services;
using System.Threading.Tasks;

namespace ProductDescriptionApi.Controllers
{
    [ApiController]
    [Route("product-description")]
    public class ProductDescriptionController : ControllerBase
    {
        private readonly OpenAIService _openAIApiService;
        private readonly string? _languageInputFilePath;
        private readonly string? _compellingInputFilePath;
        private readonly string? _constraintsInputFilePath;
        private readonly IConfiguration _configuration;
        private readonly AssessConstraintsController _assessConstraintsController;
        private readonly AssessEthicsController _assessEthicsController;
        private readonly AssessLanguageController _assessLanguageController;

        // Constructor injection of OpenAIApiService
        public ProductDescriptionController(
            IConfiguration configuration,
            OpenAIService openAIApiService,
            AssessConstraintsController assessConstraintsController,
            AssessEthicsController assessEthicsController,
            AssessLanguageController assessLanguageController)
        {
            _configuration = configuration;
            _openAIApiService = openAIApiService;
            _languageInputFilePath = _configuration["InputFilePath:Language"];
            _compellingInputFilePath = _configuration["InputFilePath:Compelling"];
            _constraintsInputFilePath = _configuration["InputFilePath:Constraints"];
            _assessConstraintsController = assessConstraintsController;
            _assessEthicsController = assessEthicsController;
            _assessLanguageController = assessLanguageController;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var response = new { Message = "Hello! You are talking to the API that will generate product descriptions using AI" };
            return Ok(response);
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateDescription([FromBody] ProductDescriptionRequest request)
        {
            // Ensure that the request parameters are not null
            string systemMessage = request.SystemMessage ?? "string";
            string userMessage = request.UserMessage ?? "Default User Message";
            string temp = request.Temperature ?? "0.7";
            string attributes = request.Attributes ?? "";

            double Temperature = double.Parse(temp);
            string messageContent = "";
            string constraintsEvaluation = "";
            string ethicsEvaluation = "";
            string languageEvaluation = "";

            int attempt = 0;
            const int maxAttempts = 3;

            do
            {
                attempt++;
                // Call the OpenAI service with the system message and the user message
                string response = await _openAIApiService.CreateChatCompletionAsync(systemMessage, userMessage, Temperature);

                // Parse the JSON to extract message.content
                var parsedResponse = JsonConvert.DeserializeObject<ApiResponse>(response);
                messageContent = parsedResponse?.choices?[0]?.message?.content ?? "No response";

                // Write the response to a CSV file
                WriteToCSV(messageContent, _languageInputFilePath);
                WriteToCSV(messageContent, _compellingInputFilePath);
                WriteToCSV($"{messageContent}----{attributes}", _constraintsInputFilePath);

                // Assess the generated description
                constraintsEvaluation = await _assessConstraintsController.AssessSingleDescription(new ProductDescription(messageContent, attributes));
                ethicsEvaluation = await _assessEthicsController.AssessSingleDescription(new ProductDescription(messageContent));
                languageEvaluation = await _assessLanguageController.AssessSingleDescription(new ProductDescription(messageContent));

                // Log the results to the console
                Console.WriteLine("Generated Product Description:");
                Console.WriteLine(messageContent);
                Console.WriteLine("Constraints Evaluation: " + constraintsEvaluation);
                Console.WriteLine("Ethics Evaluation: " + ethicsEvaluation);
                Console.WriteLine("Language Evaluation: " + languageEvaluation);
            } while ((constraintsEvaluation.ToLower() != "correct" || ethicsEvaluation.ToLower() != "correct" || languageEvaluation.ToLower() != "correct") && attempt < maxAttempts);
            // Return the evaluation results
            var result = new
            {
                Description = messageContent.ToLower(),
                ConstraintsEvaluation = constraintsEvaluation.ToLower(),
                EthicsEvaluation = ethicsEvaluation.ToLower(),
                LanguageEvaluation = languageEvaluation.ToLower()
            };

            return Ok(result);
        }

        private void WriteToCSV(string text, string filePath)
        {
            // Write the text to a CSV file (append mode)
            using var writer = new StreamWriter(filePath, append: true);
            writer.WriteLine(text);
        }
    }
}
