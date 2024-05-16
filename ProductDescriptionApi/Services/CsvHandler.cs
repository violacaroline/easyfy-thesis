using System;
using System.Collections.Generic;
using System.IO;
using ProductDescriptionApi.Models;

namespace ProductDescriptionApi.Services
{
    public class CsvHandler
    {
        public List<ProductDescription> ReadDescriptionsAndAttributesFromCSV(string filePath)
        {
            List<ProductDescription> descriptionsAndAttributes = new List<ProductDescription>();

            // Read descriptions and attributes
            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    //  Because the format is Description----Attributes
                    var parts = line?.Split(new string[] { "----" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts?.Length == 1) // Ensure there's a description part.
                    {
                        string description = parts[0].Trim();
                        descriptionsAndAttributes.Add(new ProductDescription(description));
                    }
                    if (parts?.Length == 2) // Ensure there's a description and attributes part.
                    {
                        string description = parts[0].Trim();
                        string attributes = parts[1].Trim();
                        descriptionsAndAttributes.Add(new ProductDescription(description, attributes));
                    }
                }
            }

            return descriptionsAndAttributes;
        }

        public void WriteAssessmentsResultsToCSV(List<Tuple<int, string, string>> assessmentsResults, string filePath)
        {
            using var writer = new StreamWriter(filePath, append: false); // Open file, ready to overwrite old data
            writer.WriteLine("Product Number;Product Description;Evaluation");  // Write the header

            foreach (var result in assessmentsResults)
            {
                writer.WriteLine($"{result.Item1};{result.Item2};{result.Item3}");
            }
        }
    }
}

