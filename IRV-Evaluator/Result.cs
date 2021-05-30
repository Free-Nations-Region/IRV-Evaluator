using System.Collections.Generic;

namespace IRV_Evaluator
{
    public class Result
    {
        public Office Office { get; set; }
        public List<Round> Rounds { get; set; } = new List<Round>();
    }
}