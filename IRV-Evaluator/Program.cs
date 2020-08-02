using GenericParsing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace IRV_Evaluator
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

                var fileSettings = configuration.GetSection("FileSettings").Get<FileSettings>();
                var offices = configuration.GetSection("Offices").Get<IEnumerable<Office>>();
                using (var parser = new GenericParser(fileSettings.Path))
                {
                    parser.FirstRowHasHeader = fileSettings.FirstRowHasHeader;
                    parser.ColumnDelimiter = fileSettings.Delimiter;
                    parser.FirstRowSetsExpectedColumnCount = true;
                    parser.TrimResults = true;
                    while (parser.Read())
                    {
                        foreach (var office in offices)
                        {
                            office.ReadVote(parser);
                        }
                    }
                }
                foreach (var office in offices)
                {
                    Evaluator.Evaluate(office);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"A fatal error occurred.{Environment.NewLine}{ex}");
            }
            Console.WriteLine("Evaluation complete");
            Console.ReadKey();
        }
    }
}