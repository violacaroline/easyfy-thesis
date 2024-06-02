using Microsoft.AspNetCore.Mvc;
using ProductDescriptionApi.Models;
using ProductDescriptionApi.Services;

namespace ProductDescriptionApi.Controllers;
[ApiController]
[Route("ethics-assessment")]
public class AssessEthicsController : ControllerBase
{
  private readonly PDEService _pdeService;
  private readonly ILogger<AssessEthicsController> _logger;

  public AssessEthicsController(PDEService pdeService, ILogger<AssessEthicsController> logger)
  {
    _pdeService = pdeService;
    _logger = logger;
  }

  [HttpPost("assess")]
  public async Task<IActionResult> AssessDescriptions([FromBody] ProductDescription request)
  {
    if (request == null || string.IsNullOrEmpty(request.Description))
    {
      _logger.LogWarning("Invalid request payload.");
      return BadRequest("Invalid request payload.");
    }

    string assessingPrompt = $"Kontrollera om följande produktbeskrivning är övertygande eller inte övertygande, ta hänsyn till etiska aspekter gällande marknadsföring. Returnera endast \'correct\' om du anser att texten är övertygande utan att ge några ytterligare kommentarer eller detaljer. Returnera endast \"wrong\" om du anser att texten inte är övertygande utan att ge några ytterligare kommentarer eller detaljer. Jag är intresserad av att förstå om texten effektivt lockar kunden och framhäver produkten på ett positivt sätt. Tack!";

    string productDescription = request.Description;

    try
    {
      var productDescriptionEvaluation = await _pdeService.AssessDescriptionAsync(assessingPrompt, productDescription);

      if (productDescriptionEvaluation == null)
      {
        _logger.LogError("Error assessing product description: Evaluation returned null.");
        return StatusCode(500, "Error assessing product description.");
      }

      return Ok(productDescriptionEvaluation);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Exception occurred while assessing product description.");
      return StatusCode(500, "Internal server error occurred while assessing product description.");
    }
  }
}