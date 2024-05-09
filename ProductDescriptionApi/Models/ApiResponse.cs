using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductDescriptionApi.Models
{
  public class ApiResponse
  {
    public List<Choice>? choices { get; set; }
    public Usage? usage { get; set; }
  }

  public class Usage
  {
    long TotalTokens { get; set; }
  }

  public class Choice
  {
    public Message? message { get; set; }
  }

  public class Message
  {
    public string? content { get; set; }
  }
}