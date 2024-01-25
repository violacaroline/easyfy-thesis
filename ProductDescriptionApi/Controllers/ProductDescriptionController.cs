using Microsoft.AspNetCore.Mvc;

namespace ProductDescriptionApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductDescriptionController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
       var response = new { Message = "Welcome to this api " };

            // Return the anonymous object as a JSON result
            return Ok(response);
    }

    
}
