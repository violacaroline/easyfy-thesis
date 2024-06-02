using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProductDescriptionApi.Models;
namespace ProductDescriptionApi.Controllers;
using ProductDescriptionApi.Services;

[ApiController]
[Route("constraints-assessment")]

public class AssessConstraintsController : ControllerBase

{
  private readonly PDEService _pdeService;

  public AssessConstraintsController(PDEService pdeService)
  {
    _pdeService = pdeService;
  }

  [HttpPost("assess")]
  public async Task<IActionResult> AssessDescriptions([FromBody] ProductDescription request)
  {
    string assessingPrompt = $"Vänligen verifiera att den tillhandahållna texten innehåller alla de angivna orden, fraserna och formuleringarna som listas i [{request.Attributes}], eller deras synonymer och andra formuleringar som förmedlar samma betydelse. Svara med 'correct' om texten innehåller motsvarigheter för varje punkt på listan, antingen som specificerat eller genom godtagbara alternativ. Svara med 'wrong' om någon motsvarighet saknas eller inte adekvat förmedlar samma betydelse. Undvik att ge några ytterligare kommentarer eller detaljer.";

    string productDescription = $"Text: {request.Description}";
    var productDescriptionEvaluation = await _pdeService.AssessDescriptionAsync(assessingPrompt, productDescription);

    return Ok(productDescriptionEvaluation);
  }

}