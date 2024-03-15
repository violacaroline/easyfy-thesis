using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProductDescriptionApi.Models;
using System.Threading.Tasks;

namespace ProductDescriptionApi.Controllers;

public class AssessLanguageController : ControllerBase
{
  // Read a CSV file
  // Prompta ChatGPT in loop -> Returnerar "Correct" OR *new corrected description*
  // Save results to CSV - if correct - save original description, if incorrect - save new description 
}