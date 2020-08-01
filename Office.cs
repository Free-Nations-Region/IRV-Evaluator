using GenericParsing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IRV_Evaluator
{
    public class Office
    {
        public string Name { get; set; }
        public List<Candidate> Candidates { get; set; } = new List<Candidate>();
        public List<Vote> Votes { get; set; } = new List<Vote>();
        public int ValidVoteCount => Votes.Count(v => v.IsValid);
        public int StartIndex { get; set; }

        public void LoadCandidates()
        {
            foreach (Candidate candidate in Candidates)
            {
                candidate.Votes = Votes
                    .Where(v => v.IsValid)
                    .Select(v => v.GetRankingByIndex(candidate.Index)).ToList();
                candidate.Office = this;
            }
        }
        public void ReadVote(GenericParser parser)
        {
            var vote = new Vote() { IsValid = true };
            for (int i = StartIndex; i < StartIndex + Candidates.Count; i++)
            {
                var parsed = int.TryParse(parser[i], out int parsedValue);
                if (!parsed)
                {
                    parsedValue = -1;
                    Console.WriteLine($"Value couldn't be parsed. Row: {parser.DataRowNumber} Column: {i}");
                }
                vote.Rankings.Add(parsedValue);
            }
            Votes.Add(vote);
        }
        public void RecalculatePercentage()
        {
            Candidates.ForEach(c => c.Percentage = c.VoteCount * 100.00 / ValidVoteCount);
        }
    }
}