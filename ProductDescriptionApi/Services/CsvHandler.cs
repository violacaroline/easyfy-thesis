using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductDescriptionApi.Services
{
    public class CsvHandler
    {

        static List<string> ReadDescriptionsFromCSV(string filePath)
        {
            List<string> descriptions = new List<string>();

            // Read descriptions from CSV file
            using (var reader = new StreamReader(filePath))
            {
                // Skip header
                reader.ReadLine();

                // Read each line and extract description
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    if (values.Length > 0)
                    {
                        string description = values[0].Trim();
                        descriptions.Add(description);
                    }
                }
            }

            return descriptions;
        }
    }
}