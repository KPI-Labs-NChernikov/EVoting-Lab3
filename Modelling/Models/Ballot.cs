namespace Modelling.Models;
public sealed class Ballot
{
    public Guid VoterId { get; }
    public Guid RegistrationId { get; }
    public int CandidateId { get; }

    public Ballot(Guid voterId, Guid registrationId, int candidateId)
    {
        VoterId = voterId;
        RegistrationId = registrationId;
        CandidateId = candidateId;
    }

}
