using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductDescriptionApi.Models
{
    public class ProductDescriptionRequest
    {
        public string? productAttributes { get; set; }
        public string? Temperature { get; set; }
    }
}