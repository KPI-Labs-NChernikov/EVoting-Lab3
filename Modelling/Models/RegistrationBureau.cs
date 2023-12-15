using Algorithms.Abstractions;
using Algorithms.Common;
using FluentResults;
using Org.BouncyCastle.Crypto;
using System.Security.Cryptography;

namespace Modelling.Models;
public sealed class RegistrationBureau : BaseSecuredEntity
{
    public bool IsRegistrationClosed { get; private set; }

    private readonly Dictionary<string, (Voter, Guid)> _voters = [];

    public RegistrationBureau(IEnumerable<Voter> voters, Keys<DSAParameters> signatureKeys, Keys<AsymmetricKeyParameter> encryptionKeys, ISignatureProvider<DSAParameters> signatureProvider, IEncryptionProvider<AsymmetricKeyParameter> encryptionProvider, IObjectToByteArrayTransformer transformer)
        : base(signatureKeys, encryptionKeys, signatureProvider, encryptionProvider, transformer)
    {
        foreach (var voter in voters)
        {
            _voters[voter.FullName] = (voter, Guid.Empty);
        }
    }

    public Result<byte[]> RequestRegistrationNumber(byte[] request, AsymmetricKeyParameter voterEncryptionPublicKey)
    {
        return CheckIfRegistrationIsClosed()
            .Bind(() => DecryptSignedName(request))
            .Bind(VerifyVoter)
            .Bind(GenerateRegistrationNumber)
            .Bind(rn => SignAndEncryptRegistrationNumber(rn, voterEncryptionPublicKey));
    }

    private Result CheckIfRegistrationIsClosed()
    {
        return Result.FailIf(IsRegistrationClosed, new Error("The registration is closed."));
    }

    private Result<SignedData<string>> DecryptSignedName(byte[] request)
    {
        return Result.Try(()
                => transformer.ReverseTransform<SignedData<string>>(encryptionProvider.Decrypt(request, encryptionPrivateKey))
                    ?? throw new InvalidOperationException("Value cannot be transformed to signed name."),
                e => new Error("Message has wrong format or was incorrectly encrypted.").CausedBy(e));
    }

    private Result<string> VerifyVoter(SignedData<string> signedVoterName)
    {
        var voterWasFound = _voters.TryGetValue(signedVoterName.Data, out var voter);

        if (!voterWasFound)
        {
            return Result.Fail("Voter was not found");
        }

        var voterHasRightToVote = voter.Item1.IsAbleToVote();
        if (voterHasRightToVote.IsFailed)
        {
            return voterHasRightToVote;
        }

        var signatureIsAuthentic = signatureProvider.Verify(transformer.Transform(signedVoterName.Data), signedVoterName.Signature, voter.Item1.SignaturePublicKey);
        if (!signatureIsAuthentic)
        {
            return Result.Fail(new Error("The signature is not authentic."));
        }

        if (voter.Item2 != Guid.Empty)
        {
            return Result.Fail(new Error("Voter has already requested an id."));
        }

        return Result.Ok(signedVoterName.Data);
    }

    private Result<Guid> GenerateRegistrationNumber(string voterName)
    {
        var registrationNumber = Guid.NewGuid();
        _voters[voterName] = (_voters[voterName].Item1, registrationNumber);

        return Result.Ok(registrationNumber);
    }

    private Result<byte[]> SignAndEncryptRegistrationNumber(Guid registrationNumber, AsymmetricKeyParameter voterEncryptionPublicKey)
    {
        var signature = signatureProvider.Sign(transformer.Transform(registrationNumber), signaturePrivateKey);
        var signedRegNumber = new SignedData<Guid>(registrationNumber, signature);
        return encryptionProvider.Encrypt(transformer.Transform(signedRegNumber), voterEncryptionPublicKey);
    }

    public Dictionary<Guid, DSAParameters> FinishRegistration()
    {
        IsRegistrationClosed = true;
        return _voters.Values.Where(e => e.Item2 != Guid.Empty)
            .ToDictionary(e => e.Item2, e => e.Item1.SignaturePublicKey);
    }
}
