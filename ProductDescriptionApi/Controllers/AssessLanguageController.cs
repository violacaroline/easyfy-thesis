using Microsoft.AspNetCore.Mvc;
using ProductDescriptionApi.Models;
using ProductDescriptionApi.Services;

namespace ProductDescriptionApi.Controllers
{
    [ApiController]
    [Route("language-assessment")]
    public class AssessLanguageController : ControllerBase
    {
        private readonly PDEService _pdeService;
        private readonly ILogger<AssessLanguageController> _logger;

        public AssessLanguageController(PDEService pdeService, ILogger<AssessLanguageController> logger)
        {
            _pdeService = pdeService;
            _logger = logger;
        }

        /// <summary>
        /// Assesses the product description for language correctness.
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

            string assessingPrompt = "Bedöm om följande text innehåller några stavfel, grammatiska fel eller fel skiljetecken. Säkerställ att possessiva pronomenen passar substantiven. Returnera endast 'Wrong' om texten behöver korrigeringar, och 'Correct' om texten är korrekt. utan att ge några ytterligare kommentarer eller detaljer.";

            string productDescription = request.Description;

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
}