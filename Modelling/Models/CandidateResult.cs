namespace Modelling.Models;
public sealed class CandidateResult
{
    public Candidate Candidate { get; }

    public int Votes { get; set; }

    public CandidateResult(Candidate candidate)
    {
        Candidate = candidate;
    }
}
