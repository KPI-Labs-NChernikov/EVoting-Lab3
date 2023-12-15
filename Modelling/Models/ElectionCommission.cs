using Algorithms.Abstractions;
using Algorithms.Common;
using FluentResults;
using Org.BouncyCastle.Crypto;
using System.Security.Cryptography;

namespace Modelling.Models;
public sealed class ElectionCommission : BaseSecuredEntity
{
    private readonly Dictionary<Guid, DSAParameters> _voters;

    public VotingResults VotingResults { get; } = new();

    public bool IsVotingCompleted { get; private set; }

    public ElectionCommission(IEnumerable<Candidate> candidates, Dictionary<Guid, DSAParameters> voters, Keys<DSAParameters> signatureKeys, Keys<AsymmetricKeyParameter> encryptionKeys, ISignatureProvider<DSAParameters> signatureProvider, IEncryptionProvider<AsymmetricKeyParameter> encryptionProvider, IObjectToByteArrayTransformer transformer)
        : base(signatureKeys, encryptionKeys, signatureProvider, encryptionProvider, transformer)
    {
        foreach (var candidate in candidates)
        {
            VotingResults.CandidatesResults.Add(candidate.Id, new(candidate));
        }

        _voters = voters;
    }

    public Result AcceptVote(byte[] encryptedSignedBallot)
    {
        return CheckIfVotingIsCompleted()
            .Bind(() => DecryptSignedBallot(encryptedSignedBallot))
            .Bind(VerifyVoter)
            .Bind(VerifyCandidate)
            .Bind(AddVote);
    }

    private Result CheckIfVotingIsCompleted()
    {
        return Result.FailIf(IsVotingCompleted, new Error("The voting is already completed."));
    }

    private Result<SignedData<Ballot>> DecryptSignedBallot(byte[] encryptedSignedBallot)
    {
        return Result.Try(()
                => transformer.ReverseTransform<SignedData<Ballot>>(encryptionProvider.Decrypt(encryptedSignedBallot, encryptionPrivateKey))
                    ?? throw new InvalidOperationException("Value cannot be transformed to signed ballot."),
                e => new Error("Message has wrong format or was incorrectly encrypted.").CausedBy(e));
    }

    private Result<Ballot> VerifyVoter(SignedData<Ballot> signedBallot)
    {
        var voterWasFound = _voters.TryGetValue(signedBallot.Data.RegistrationId, out var voterSignaturePublicKey);

        if (!voterWasFound)
        {
            return Result.Fail("Voter has not registered or has already casted a vote.");
        }

        var signatureIsAuthentic = signatureProvider.Verify(transformer.Transform(signedBallot.Data), signedBallot.Signature, voterSignaturePublicKey);
        if (!signatureIsAuthentic)
        {
            return Result.Fail(new Error("The signature is not authentic."));
        }

        if (signedBallot.Data.VoterId == Guid.Empty)
        {
            return Result.Fail(new Error("Voter id cannot be empty."));
        }

        return Result.Ok(signedBallot.Data);
    }

    private Result<Ballot> VerifyCandidate(Ballot ballot)
    {
        var candidateWasFound = VotingResults.CandidatesResults.ContainsKey(ballot.CandidateId);

        if (!candidateWasFound)
        {
            return Result.Fail(new Error("Candidate was not found."));
        }

        return Result.Ok(ballot);
    }

    private Result AddVote(Ballot ballot)
    {
        _voters.Remove(ballot.RegistrationId);

        VotingResults.CandidatesResults[ballot.CandidateId].Votes++;

        VotingResults.VotersResults.Add(new(ballot.VoterId, ballot.CandidateId));

        return Result.Ok();
    }

    public void CompleteVoting()
    {
        IsVotingCompleted = true;
    }
}
