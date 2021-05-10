using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IRV_Evaluator
{
    public static class Evaluator
    {
        public static double _threshold = 66.67;

        public static Result Evaluate(Office office)
        {
            var result = new Result();
            office.LoadCandidates();
            office.Candidates.ForEach(c =>
            {
                c.VoteCount += c.CountRanking(1);
            });
            office.RecalculatePercentage();
            Console.WriteLine($"{office.Name} - Votes: {office.Votes.Count} Valid Votes: {office.ValidVoteCount}");
            var round = EvaluateRound(office.Candidates, out bool majorityFound);
            result.Rounds.Add(round);
            round.Print($"{office.Name} - Round {result.Rounds.Count}");
            var roundCounter = 1;
            while (!majorityFound)
            {
                var candidate = EliminateCandidate(office.Candidates);
                candidate.EliminatedInRound = roundCounter;
                RedistributeVotes(candidate, roundCounter);
                round = EvaluateRound(office.Candidates, out majorityFound);
                //majorityFound = office.Candidates.Max(c => c.Percentage) > 66.6;
                result.Rounds.Add(round);
                round.Print($"{office.Name} - Round {result.Rounds.Count}");
                roundCounter++;
            }
            return result;
        }

        private static Round EvaluateRound(List<Candidate> candidates, out bool majorityFound)
        {
            var round = new Round();
            var orderedCandidates = candidates.Where(c => !c.IsEliminated).OrderByDescending(c => c.VoteCount).ToList();
            for (int i = 0; i < orderedCandidates.Count; i++)
            {
                round.Percentages.Add(orderedCandidates[i].Name, new Tuple<int, double>(orderedCandidates[i].VoteCount, orderedCandidates[i].Percentage));
            }
            majorityFound = orderedCandidates.Max(c => c.Percentage) > _threshold;
            return round;
        }

        private static void RedistributeVotes(Candidate eliminated, int rank)
        {
            if (rank > 1)
            {
                RedistributeVotes(eliminated, rank - 1);
            }
            var office = eliminated.Office;
            if (rank < office.Candidates.Count)
            {
                var voteIndizes = eliminated.GetIndizesOfAllVotesWithRanking(rank);
                var redistributed = office.Candidates.Select(c => c.GetVotesByIndizes(voteIndizes).Count(v => v == rank + 1)).ToList();
                for (int i = 0; i < office.Candidates.Count; i++)
                {
                    if (redistributed[i] == 0)
                    {
                        continue;
                    }
                    var candiate = office.Candidates[i];
                    if (!candiate.IsEliminated)
                    {
                        candiate.VoteCount += redistributed[i];
                    }
                }
                office.RecalculatePercentage();
            }
            else
            {
                throw new InvalidOperationException($"Unable to redistribute vote on last rank. Candidate: {eliminated.Name} Rank: {rank}");
            }
        }

        private static Candidate EliminateCandidate(List<Candidate> candidates)
        {
            var toEliminate = GetCandidateToEliminate(candidates, 1);
            toEliminate.IsEliminated = true;
            Console.WriteLine($"{toEliminate.Name} eliminated.");
            return toEliminate;
        }

        private static Candidate GetCandidateToEliminate(IEnumerable<Candidate> candidates, int start)
        {
            var notEliminated = candidates.Where(c => !c.IsEliminated);
            var min = notEliminated.Min(c => c.VoteCount);
            var toEliminate = candidates.Where(c => c.VoteCount == min);
            if (start == 1)
            {
                foreach (var eliminate in toEliminate)
                {
                    Console.WriteLine($"To Eliminate: {eliminate.Name} Votes: {eliminate.VoteCount}");
                }
            }

            if (toEliminate.Count() > 1)
            {
                Console.WriteLine($"More then 1 candidate to eliminate. Considering #{start} votes.");
                var ranking = toEliminate.Select(c => new { Candidate = c, Votes = c.CountRanking(start) }).OrderBy(c => c.Votes).ToList();
                foreach (var c in ranking)
                {
                    Console.WriteLine($"{c.Candidate.Name} - {c.Votes} #{start} votes.");
                }
                var group = ranking.GroupBy(x => x.Votes);
                var firstDuplicate = group.First().Count() > 1;

                if (!firstDuplicate)
                {
                    return ranking.First().Candidate;
                }
                else
                {
                    toEliminate = group.First().Select(x => x.Candidate);
                    return GetCandidateToEliminate(toEliminate, start + 1);
                }
            }
            else
            {
                return toEliminate.First();
            }
        }
    }
}