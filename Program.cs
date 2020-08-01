using GenericParsing;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace IRV_Evaluator
{
    class Program
    {
        static void Main(string[] args)
        {
            string path;
            Office office;
            Setup(out path, out office);
            using (var parser = new GenericParser(path))
            {
                parser.FirstRowHasHeader = true;
                parser.FirstRowSetsExpectedColumnCount = true;
                parser.TrimResults = true;
                while (parser.Read())
                {
                    office.ReadVote(parser);
                }
            }
            var res = Evaluator.Evaluate(office);
            Console.ReadKey();
        }

        private static void Setup(out string path, out Office office)
        {
            path = @"C:\Users\Drehtisch\source\repos\IRV-Evaluator\test.CSV";
            office = new Office()
            {
                Name = "Test",
                StartIndex = 0,
                Candidates = new List<Candidate>()
                {
                    new Candidate() { Name = "Candidate 1", Index = 0 },
                    new Candidate() { Name = "Candidate 2", Index = 1 },
                    new Candidate() { Name = "Candidate 3", Index = 2 },
                    new Candidate() { Name = "Candidate 4", Index = 3 },
                }
            };
        }
    }
}