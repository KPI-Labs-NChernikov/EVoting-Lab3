using Modelling.Extensions;
using Modelling.Models;
using static Demo.UtilityMethods;

namespace Demo;
public sealed class ModellingPrinter
{
    public void PrintUsualRegistration(RegistrationBureau registrationBureau, IEnumerable<Voter> voters)
    {
        Console.WriteLine("Usual registration:");

        Parallel.ForEach(voters, voter =>
        {
            var request = voter.PrepareRegistrationNumberRequest(registrationBureau.EncryptionPublicKey);
            var registrationNumber = registrationBureau.RequestRegistrationNumber(request, voter.EncryptionPublicKey);

            if (registrationNumber.IsFailed)
            {
                PrintError(registrationNumber);
                return;
            }

            voter.SetRegistrationNumber(registrationNumber.Value, registrationBureau.SignaturePublicKey);
            Console.WriteLine($"Voter {voter.FullName} has just registered. Their registration number: {voter.RegistrationNumber}.");
        });

        //foreach (var voter in voters)
        //{
        //    var request = voter.PrepareRegistrationNumberRequest(registrationBureau.EncryptionPublicKey);
        //    var registrationNumber = registrationBureau.RequestRegistrationNumber(request, voter.EncryptionPublicKey);

        //    if (registrationNumber.IsFailed)
        //    {
        //        PrintError(registrationNumber);
        //        continue;
        //    }

        //    voter.SetRegistrationNumber(registrationNumber.Value, registrationBureau.SignaturePublicKey);
        //    Console.WriteLine($"Voter {voter.FullName} has just registered. Their registration number: {voter.RegistrationNumber}.");
        //}

        Console.WriteLine();
    }

    public void PrintDoubleRegistration(RegistrationBureau registrationBureau, Voter voter)
    {
        Console.WriteLine("Trying to register the same voter two times:");

        var request = voter.PrepareRegistrationNumberRequest(registrationBureau.EncryptionPublicKey);
        var registrationNumber = registrationBureau.RequestRegistrationNumber(request, voter.EncryptionPublicKey);

        registrationNumber.PrintErrorIfFailed();

        Console.WriteLine();
    }

    public void PrintUsualVoting(ElectionCommission electionCommission, Dictionary<Voter, int> votersWithCandidates)
    {
        Console.WriteLine("Usual voting:");

        foreach(var (voter, candidateId) in votersWithCandidates)
        {
            voter.GenerateAndSetId();
            var ballot = voter.PrepareBallot(candidateId, electionCommission.EncryptionPublicKey);
            var result = electionCommission.AcceptVote(ballot);

            if (result.IsSuccess)
            {
                Console.WriteLine($"Voter (reg: {voter.RegistrationNumber}, gen id: {voter.Id}) has casted their vote.");
            }
            else
            {
                PrintError(result);
            }
        }

        Console.WriteLine();
    }

    public void PrintVotingWithIncorrectBallot(ElectionCommission electionCommission, Voter voter)
    {
        Console.WriteLine("Voting with incorrect ballots:");
        var finalBallot = new byte[] { 4, 6, 8, 0 };
        var result = electionCommission.AcceptVote(finalBallot);
        result.PrintErrorIfFailed();

        voter.GenerateAndSetId();
        finalBallot = voter.PrepareBallot(100, electionCommission.EncryptionPublicKey);
        result = electionCommission.AcceptVote(finalBallot);
        result.PrintErrorIfFailed();

        Console.WriteLine();
    }

    public void PrintVotingWithDoubleBallot(ElectionCommission electionCommission, int candidateId, Voter voter)
    {
        Console.WriteLine("Trying to vote two times:");
        var ballot = voter.PrepareBallot(candidateId, electionCommission.EncryptionPublicKey);
        var result = electionCommission.AcceptVote(ballot);
        if (result.IsSuccess)
        {
            Console.WriteLine("Vote has been accepted for the first time.");
            Console.WriteLine($"Voter (reg: {voter.RegistrationNumber}, gen id: {voter.Id}) has casted their vote.");
        }
        else
        {
            PrintError(result);
        }

        ballot = voter.PrepareBallot(candidateId, electionCommission.EncryptionPublicKey);
        result = electionCommission.AcceptVote(ballot);
        result.PrintErrorIfFailed();

        Console.WriteLine();
    }

    public void PrintVotingResults(ElectionCommission commission)
    {
        Console.WriteLine("Results:");
        commission.CompleteVoting();

        var results = commission.VotingResults;
        Console.WriteLine("Ballots:");
        foreach (var ballotResult in results.VotersResults)
        {
            Console.WriteLine($"Voter ID: {ballotResult.VoterId} Candidate: {ballotResult.CandidateId}");
        }
        Console.WriteLine("Candidates:");
        foreach (var candidate in results.CandidatesResults.Values.OrderByVotes())
        {
            Console.WriteLine($"{candidate.Candidate.FullName} (id: {candidate.Candidate.Id}): {candidate.Votes} votes");
        }
        Console.WriteLine();
    }

    public void PrintVotingAfterCompletion(ElectionCommission commission, int candidateId, Voter voter)
    {
        Console.WriteLine("Trying to vote after the completion of voting:");
        if (!commission.IsVotingCompleted)
        {
            commission.CompleteVoting();
        }

        var ballot = voter.PrepareBallot(candidateId, commission.EncryptionPublicKey);
        var result = commission.AcceptVote(ballot);
        result.PrintErrorIfFailed();

        Console.WriteLine();
    }
}
