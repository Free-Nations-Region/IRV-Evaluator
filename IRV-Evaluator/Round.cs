using System;
using System.Collections.Generic;

namespace IRV_Evaluator
{
    public class Round
    {
        public Dictionary<string, Tuple<int, double>> Percentages { get; set; } = new Dictionary<string, Tuple<int, double>>();

        public void Print(string name)
        {
            string head = $"===== {name} =====";
            Console.WriteLine();
            Console.WriteLine(head);
            Console.WriteLine();
            foreach (var res in Percentages)
            {
                Console.WriteLine($"{res.Key} | {res.Value.Item1} - {res.Value.Item2:0.00} %");
            }
            Console.WriteLine();
            Console.WriteLine(new string('=', head.Length));
            Console.WriteLine();
        }
    }
}