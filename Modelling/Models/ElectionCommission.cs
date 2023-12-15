using Algorithms.Abstractions;
using Algorithms.Common;
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

    public void CompleteVoting()
    {
        IsVotingCompleted = true;
    }
}
