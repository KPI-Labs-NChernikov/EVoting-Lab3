namespace Modelling.Models;
public sealed class VoterResult
{
    public Guid VoterId { get; }
    public int CandidateId { get; }

    public VoterResult(Guid voterId, int candidateId)
    {
        VoterId = voterId;
        CandidateId = candidateId;
    }
}
