using Microsoft.AspNetCore.Mvc;

namespace ProductDescriptionApi.Controllers;

[ApiController]
// [Route("[controller]")]
[Route("product-description")]
public class ProductDescriptionController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
       var response = new { Message = "Hello! You are talking to the API that will generate product descriptions using AI" };

            // Return the anonymous object as a JSON result
            return Ok(response);
    }    
}
