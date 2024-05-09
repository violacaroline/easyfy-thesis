using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductDescriptionApi.Models
{
    public class ProductDescriptionRequest
    {
        public string? SystemMessage { get; set; }
        public string? UserMessage { get; set; }
        public string? Attributes { get; set; }
        public string? Temperature { get; set; }
    }
}