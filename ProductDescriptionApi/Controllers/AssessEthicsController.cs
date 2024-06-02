using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProductDescriptionApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using ProductDescriptionApi.Services;

namespace ProductDescriptionApi.Controllers;

[ApiController]
[Route("ethics-assessment")]
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
    string assessingPrompt = $"Kontrollera om följande produktbeskrivning är övertygande eller inte övertygande, ta hänsyn till etiska aspekter gällande marknadsföring. Returnera endast \'correct\' om du anser att texten är övertygande utan att ge några ytterligare kommentarer eller detaljer. Returnera endast \"wrong\" om du anser att texten inte är övertygande utan att ge några ytterligare kommentarer eller detaljer. Jag är intresserad av att förstå om texten effektivt lockar kunden och framhäver produkten på ett positivt sätt. Tack!";

    string productDescription = request.Description;

    var productDescriptionEvaluation = await _pdeService.AssessDescriptionAsync(assessingPrompt, productDescription);

    return Ok(productDescriptionEvaluation);
  }
}