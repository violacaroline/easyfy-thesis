using Microsoft.AspNetCore.Mvc;
using ProductDescriptionApi.Models;
using ProductDescriptionApi.Services;


namespace ProductDescriptionApi.Controllers;

[ApiController]
[Route("constraints-assessment")]

public class AssessConstraintsController : ControllerBase
{
  private readonly PDEService _pdeService;
  private readonly ILogger<AssessConstraintsController> _logger;

  public AssessConstraintsController(PDEService pdeService, ILogger<AssessConstraintsController> logger)
  {
    _pdeService = pdeService;
    _logger = logger;
  }

  /// <summary>
  /// Assesses the product description against provided attributes.
  /// </summary>
  /// <param name="request">The product description request.</param>
  /// <returns>The assessment result.</returns>
  [HttpPost("assess")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> AssessDescriptions([FromBody] ProductDescription request)
  {
    if (request == null || string.IsNullOrEmpty(request.Description))
    {
      _logger.LogWarning("Invalid request payload.");
      return BadRequest("Invalid request payload.");
    }

    string assessingPrompt = $"Vänligen verifiera att den tillhandahållna texten innehåller alla de angivna orden, fraserna och formuleringarna som listas i [{request.Attributes}], eller deras synonymer och andra formuleringar som förmedlar samma betydelse. Svara med 'correct' om texten innehåller motsvarigheter för varje punkt på listan, antingen som specificerat eller genom godtagbara alternativ. Svara med 'wrong' om någon motsvarighet saknas eller inte adekvat förmedlar samma betydelse. Undvik att ge några ytterligare kommentarer eller detaljer.";

    string productDescription = $"Text: {request.Description}";

    try
    {
      var productDescriptionEvaluation = await _pdeService.AssessDescriptionAsync(assessingPrompt, productDescription);

      if (productDescriptionEvaluation == null)
      {
        _logger.LogError("Error assessing product description: Evaluation returned null.");
        return StatusCode(StatusCodes.Status500InternalServerError, "Error assessing product description.");
      }

      return Ok(productDescriptionEvaluation);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Exception occurred while assessing product description.");
      return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error occurred while assessing product description.");
    }
  }
}