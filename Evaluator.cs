using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IRV_Evaluator
{
    public static class Evaluator
    {
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
                var candidate = EliminateCandidate(office.Candidates, roundCounter);
                candidate.EliminatedInRound = roundCounter;
                RedistributeVotes(candidate, roundCounter);
                round = EvaluateRound(office.Candidates, out majorityFound);
                majorityFound = office.Candidates.Max(c => c.Percentage) > 50;
                result.Rounds.Add(round);
                round.Print($"{office.Name} - Round {result.Rounds.Count}");
                roundCounter++;
            }
            return result;
        }

        private static Round EvaluateRound(List<Candidate> candidates, out bool majorityFound)
        {
            var round = new Round();
            var orderedCandidates = candidates.OrderByDescending(c => c.VoteCount).ToList();
            for (int i = 0; i < orderedCandidates.Count; i++)
            {
                if (!orderedCandidates[i].IsEliminated)
                {
                    round.Percentages.Add(orderedCandidates[i].Name, new Tuple<int, double>(orderedCandidates[i].VoteCount, orderedCandidates[i].Percentage));
                }
            }
            majorityFound = orderedCandidates.Max(c => c.Percentage) > 50.0;
            return round;
        }

        private static void RedistributeVotes(Candidate eliminated, int rank)
        {
            if(rank > 1)
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
        private static Candidate EliminateCandidate(List<Candidate> orderedCandidates, int start = 1)
        {
            List<Candidate> eliminated = new List<Candidate>();
            int counter = start;
            do
            {
                if (counter > 1)
                {
                    Console.WriteLine($"More then 1 candidate to eliminate considering #{counter} vote");
                }
                eliminated = GetCandidatesToEliminateByRank(orderedCandidates, counter);
                eliminated.ForEach(c => Console.WriteLine($"To eliminate: {c.Name}"));
                if (counter == orderedCandidates.Count && eliminated.Count > 1)
                {
                    throw new InvalidOperationException("Unable to eliminate a single candidate.");
                }
                counter++;
            }
            while (eliminated.Count > 1);
            var toEliminate = eliminated.First();
            toEliminate.IsEliminated = true;
            Console.WriteLine($"{toEliminate.Name} eliminated");
            return toEliminate;
        }

        private static List<Candidate> GetCandidatesToEliminateByRank(List<Candidate> candidates, int rank)
        {
            var minVotes = candidates.Min(c => c.CountRanking(rank));
            var toEliminate = candidates.FindAll(c => c.CountRanking(rank) == minVotes);
            return toEliminate;
        }
    }
}