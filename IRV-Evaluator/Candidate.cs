using System;
using System.Collections.Generic;
using System.Linq;

namespace IRV_Evaluator
{
    public class Candidate
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public string HexColor { get; set; }
        public List<int> Votes { get; set; }
        public List<int> ReceivedRedestributedVoteIndizes { get; private set; } = new List<int>();
        public int EliminatedInRound { get; set; }
        public bool IsEliminated { get; set; }
        public int VoteCount { get; set; } = 0;
        public double Percentage { get; set; } = 0.00;
        public Office Office { get; set; }

        private Dictionary<int, int> rankingCache = new Dictionary<int, int>();

        public int CountRanking(int ranking)
        {
            if (rankingCache.ContainsKey(ranking))
            {
                return rankingCache[ranking];
            }
            else
            {
                var ranks = Votes.Count(v => v == ranking);
                rankingCache[ranking] = ranks;
                return ranks;
            }
        }

        public IEnumerable<int> GetIndizesOfAllVotesWithRanking(int ranking)
        {
            int lastIndex = 0;
            List<int> list = new List<int>();
            do
            {
                lastIndex = Votes.FindIndex(lastIndex, v => v == ranking);
                if (lastIndex > -1)
                {
                    list.Add(lastIndex);
                    lastIndex++;
                }
            } while (lastIndex > -1);
            return list;
        }

        public IEnumerable<int> GetVotesByIndizes(IEnumerable<int> indizes)
        {
            List<int> list = new List<int>();
            foreach (int index in indizes)
            {
                list.Add(Votes[index]);
            }
            return list;
        }

        public void ReceiveVoteRedestribution(int index, int redestributedRank)
        {
            if (!IsEliminated && Votes[index] == redestributedRank)
            {
                Console.WriteLine($"{Name} received a #{redestributedRank} vote.");
                ReceivedRedestributedVoteIndizes.Add(index);
                VoteCount++;
            }
        }
    }
}