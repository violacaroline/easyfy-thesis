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
        private readonly PDEService _pdeService;

        public AssessLanguageController(PDEService pdeService)
        {
            _pdeService = pdeService;
        }

        [HttpPost("assess")]
        public async Task<IActionResult> AssessDescriptions([FromBody] ProductDescription request)
        {
            string assessingPrompt = "Bedöm om följande text innehåller några stavfel, grammatiska fel eller fel skiljetecken. Säkerställ att possessiva pronomenen passar substantiven. Returnera endast 'Wrong' om texten behöver korrigeringar, och 'Correct' om texten är korrekt. utan att ge några ytterligare kommentarer eller detaljer.";

            string productDescription = request.Description;

            var productDescriptionEvaluation = await _pdeService.AssessDescriptionAsync(assessingPrompt, productDescription);

            return Ok(productDescriptionEvaluation);
        }
    }
}