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
        public bool DoValidation { get; set; }
        public int ValidationColumnIndex { get; set; }
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
            var vote = new Vote();
            ValidateVote(parser, vote);
            for (int i = StartIndex; i < StartIndex + Candidates.Count; i++)
            {
                ParseAndAddVote(parser, vote, i);
            }
            Votes.Add(vote);
        }

        private void ValidateVote(GenericParser parser, Vote vote)
        {
            if (DoValidation)
            {
                var validParsed = int.TryParse(parser[ValidationColumnIndex], out int isValid);
                if (!validParsed)
                {
                    vote.IsValid = false;
                    Console.WriteLine($"Value couldn't be parsed. Row: {parser.DataRowNumber} Column: {ValidationColumnIndex}");
                }
                else
                {
                    vote.IsValid = Convert.ToBoolean(isValid);
                }
            }
            else
            {
                vote.IsValid = true;
            }
        }

        private static void ParseAndAddVote(GenericParser parser, Vote vote, int i)
        {
            var val = parser[i];
            var parsed = int.TryParse(val, out int parsedValue);
            if (!parsed)
            {
                Console.WriteLine($"Value couldn't be parsed. Row: {parser.DataRowNumber} Column: {i}");
            }
            vote.Rankings.Add(parsedValue);
        }

        public void RecalculatePercentage()
        {
            Candidates.ForEach(c => c.Percentage = c.VoteCount * 100.00 / ValidVoteCount);
        }
    }
}