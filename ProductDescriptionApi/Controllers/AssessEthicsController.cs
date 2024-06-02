using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProductDescriptionApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductDescriptionApi.Controllers;

[ApiController]
[Route("compelling-assessment")]
public class AssessEthicsController : ControllerBase
{
  private readonly OpenAIService _openAIApiService;

  public AssessEthicsController( OpenAIService openAIApiService)
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
    Console.WriteLine("Chat gpt :");
    Console.WriteLine(messageContent);
    Console.WriteLine("-----------------------------");

    if (messageContent.Contains("correct", StringComparison.OrdinalIgnoreCase))
    {
      Console.WriteLine("The product description comply to ethics");
      return Ok("Correct");
    }

    else if (messageContent.Contains("wrong", StringComparison.OrdinalIgnoreCase))
    {
      Console.WriteLine("The product description can contain an ethical error");
      return Ok("Wrong");
    }
    return Ok();
  }



  private async Task<string> AssessDescriptionAsync(ProductDescription productInfo)
  {
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