using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProductDescriptionApi.Models
{
    public class ProductDescription
    {
        public string Description { get; set; }
        public string? Attributes { get; set; }
        public ProductDescription() { }
        
        [JsonConstructor]
    public ProductDescription(string description, string attributes = null)
    {
        Description = description;
        Attributes = attributes;
    }
    }
}