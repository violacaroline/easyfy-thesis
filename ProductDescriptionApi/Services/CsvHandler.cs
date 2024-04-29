using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductDescriptionApi.Services
{
    public class CsvHandler
    {

        public List<ProductDescription> ReadDescriptionsAndAttributesFromCSV(string filePath)
        {
            List<ProductDescription> descriptionsAndAttributes = new List<ProductDescription>();

            // Read descriptions, attributes, and correctness from CSV file
            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    // Assuming the format is Description----Attributes----Correctness
                    var parts = line?.Split(new string[] { "----" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts?.Length == 2) // Ensure there's a description, attributes part, and correctness
                    {
                        string description = parts[0].Trim();
                        string correctness = parts[1].Trim();
                        descriptionsAndAttributes.Add(new ProductDescription(description, correctness));
                    }
                    if (parts?.Length == 3) // Ensure there's a description, attributes part, and correctness
                    {
                        string description = parts[0].Trim();
                        string attributes = parts[1].Trim();
                        string correctness = parts[2].Trim();
                        descriptionsAndAttributes.Add(new ProductDescription(description, attributes, correctness));
                    }
                }
            }

            return descriptionsAndAttributes;
        }

        public void WriteConfusionMatrixDetailsToCSV(Dictionary<int, List<int>> confusionResults, string filePath)
        {
            using var writer = new StreamWriter(filePath, append: false); // Open file, ready to overwrite old data
            writer.WriteLine("Product Number;True Positive;False Positive;True Negative;False Negative"); // Write the header

            foreach (var result in confusionResults)
            {
                writer.Write($"{result.Key}"); // Product Number
                var metrics = result.Value;
                writer.Write($";{metrics[0]};{metrics[1]};{metrics[2]};{metrics[3]}"); // Write TP, FP, TN, FN values
                writer.WriteLine();
            }
        }

        public void InitializeCsvWithHeaders(string filePath, int iterations)
        {
            using var writer = new StreamWriter(filePath, append: false);
            writer.Write("Product Number");
            for (int i = 1; i <= iterations; i++)
            {
                writer.Write($";Iteration {i}");
            }
            writer.WriteLine();
        }

        public void WriteResultsToCSV(Dictionary<int, List<int>> results, string filePath)
        {
            using var writer = new StreamWriter(filePath, append: true);
            foreach (var result in results)
            {
                writer.Write($"{result.Key}"); // Product Number
                foreach (var assessment in result.Value)
                {
                    writer.Write($";{assessment}");
                }
                writer.WriteLine();
            }
        }
        public void WriteConfusionMatrixResultsToCSV(double result, string filePath, string assessmentType, string model)
        {
            using var writer = new StreamWriter(filePath, append: true);
            writer.Write($"{model};{assessmentType};{result}");
            writer.WriteLine();

        }
    }

    public class ProductDescription
    {
        public string Description { get; set; }
        public string? Attributes { get; set; }
        public string Correctness { get; set; }
        public ProductDescription(string description, string correctness)
        {
            Description = description;
            Correctness = correctness;
        }

        public ProductDescription(string description, string attributes, string correctness)
        {
            Description = description;
            Attributes = attributes;
            Correctness = correctness;
        }
    }



}