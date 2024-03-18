using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductDescriptionApi.Services
{
    public class CsvHandler
    {

        public List<string> ReadDescriptionsFromCSV(string filePath)
        {
            List<string> descriptions = new List<string>();

            // Read descriptions from CSV file
            using (var reader = new StreamReader(filePath))
            {


                // Read each line and extract description
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line?.Split('\n');
                    if (values?.Length > 0)
                    {
                        string description = values[0].Trim();
                        descriptions.Add(description);
                    }
                }
            }

            return descriptions;
        }
        public List<ProductDescription> ReadDescriptionsAndAttributesFromCSV(string filePath)
        {
            List<ProductDescription> descriptionsAndAttributes = new List<ProductDescription>();

            // Read descriptions and attributes from CSV file
            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var parts = line?.Split(new string[] { "----" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts?.Length == 2) // Ensure there's both a description and attributes part
                    {
                        string description = parts[0].Trim();
                        List<string> attributes = new List<string>(parts[1].Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries));
                        descriptionsAndAttributes.Add(new ProductDescription(description, attributes));
                    }
                }
            }

            foreach (var item in descriptionsAndAttributes)
            {
                Console.WriteLine($"Description: {item.Description}");
                Console.WriteLine("Attributes:");
                foreach (var attribute in item.Attributes)
                {
                    Console.WriteLine($"- {attribute}");
                }
                Console.WriteLine(); // Blank line for better readability
            }

            return descriptionsAndAttributes;
        }

        public void WriteToCSV(string text, string filePath)
        {
            // Write the text to a CSV file (append mode)
            using var writer = new StreamWriter(filePath, append: true);
            writer.WriteLine(text);
        }
    }

    public class ProductDescription
    {
        public string Description { get; set; }
        public List<string> Attributes { get; set; }

        public ProductDescription(string description, List<string> attributes)
        {
            Description = description;
            Attributes = attributes;
        }
    }

}