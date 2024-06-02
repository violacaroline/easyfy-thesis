using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProductDescriptionApi.Models;
namespace ProductDescriptionApi.Controllers;

[ApiController]
[Route("constraints-assessment")]

public class AssessConstraintsController : ControllerBase

{
  private readonly OpenAIService _openAIApiService;

  // Constructor injection of OpenAIApiService
  public AssessConstraintsController(OpenAIService openAIApiService)
  {
    _openAIApiService = openAIApiService;
  }

  [HttpPost("assess")]
  public async Task<IActionResult> AssessDescriptions([FromBody] ProductDescription request)
  {
    var response = await AssessDescriptionAsync(request);
    var messageContent = ParseApiResponse(response);

    Console.WriteLine("-----------------------------");
    Console.WriteLine("PD: ");
    Console.WriteLine(request.Description);
    Console.WriteLine("Chat GPT:");
    Console.WriteLine(messageContent);
    Console.WriteLine("-----------------------------");
    if (messageContent.Contains("correct", StringComparison.OrdinalIgnoreCase))
    {
      Console.WriteLine("The product description adheres to the constraints");
      return Ok("Correct");
    }
    else if (messageContent.Contains("wrong", StringComparison.OrdinalIgnoreCase))
    {
      Console.WriteLine("The product description contains constraint errors");
      return Ok("Wrong");
    }
    return Ok();
  }



  private async Task<string> AssessDescriptionAsync(ProductDescription productInfo)
  {
    string systemMessage = $"Vänligen verifiera att den tillhandahållna texten innehåller alla de angivna orden, fraserna och formuleringarna som listas i [{productInfo.Attributes}], eller deras synonymer och andra formuleringar som förmedlar samma betydelse. Svara med 'correct' om texten innehåller motsvarigheter för varje punkt på listan, antingen som specificerat eller genom godtagbara alternativ. Svara med 'wrong' om någon motsvarighet saknas eller inte adekvat förmedlar samma betydelse. Undvik att ge några ytterligare kommentarer eller detaljer.";
    double temperature = 1;
    try
    {

      string prompt = $"Text: {productInfo.Description}";
      Console.WriteLine($"Prompt: {prompt}");

      return await _openAIApiService.CreateChatCompletionAsync(systemMessage, prompt, temperature);
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
      var parsedResponse = JsonConvert.DeserializeObject<ApiResponse>(response);
      return parsedResponse?.choices?[0]?.message?.content ?? "No response";
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error parsing response: {ex.Message}");
      return "Error parsing response"; // Consider how you want to handle parse errors.
    }
  }

}