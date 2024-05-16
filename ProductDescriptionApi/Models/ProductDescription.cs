using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductDescriptionApi.Models
{
    public class ProductDescription
    {
        public string Description { get; set; }
        public string? Attributes { get; set; }
        public ProductDescription(string description)
        {
            Description = description;
        }

        public ProductDescription(string description, string attributes)
        {
            Description = description;
            Attributes = attributes;
        }
    }
}