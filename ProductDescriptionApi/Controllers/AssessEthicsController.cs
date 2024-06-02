using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProductDescriptionApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using ProductDescriptionApi.Services;

namespace ProductDescriptionApi.Controllers;

[ApiController]
[Route("compelling-assessment")]
public class AssessEthicsController : ControllerBase
{
private readonly PDEService _pdeService;
  public AssessEthicsController(PDEService pdeService)
  {
    _pdeService = pdeService;
  }

  [HttpPost("assess")]
  public async Task<IActionResult> AssessDescriptions([FromBody] ProductDescription request)
  {
    string systemMessage = $"Kontrollera om följande produktbeskrivning är övertygande eller inte övertygande, ta hänsyn till etiska aspekter gällande marknadsföring. Returnera endast \'correct\' om du anser att texten är övertygande utan att ge några ytterligare kommentarer eller detaljer. Returnera endast \"wrong\" om du anser att texten inte är övertygande utan att ge några ytterligare kommentarer eller detaljer. Jag är intresserad av att förstå om texten effektivt lockar kunden och framhäver produkten på ett positivt sätt. Tack!";

     var messageContent = await _pdeService.AssessDescriptionAsync(request, systemMessage);

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
}