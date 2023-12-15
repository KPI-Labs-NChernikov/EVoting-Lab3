namespace Modelling.Models;
public sealed class VotingResults
{
    public SortedDictionary<int, CandidateResult> CandidatesResults { get; } = [];

    public ICollection<VoterResult> VotersResults { get; } = new List<VoterResult>();
}
