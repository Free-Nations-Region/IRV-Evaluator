using System;
using System.Collections.Generic;

namespace IRV_Evaluator
{
    public class Vote
    {
        public List<int> Rankings { get; } = new List<int>();
        public bool IsValid { get; set; }

        public int GetRankingByIndex(int index)
        {
            if (index < 0 || index >= Rankings.Count) throw new ArgumentOutOfRangeException(nameof(index), actualValue: index, null);
            return Rankings[index];
        }
    }
}