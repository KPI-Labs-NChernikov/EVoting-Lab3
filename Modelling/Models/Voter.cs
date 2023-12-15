using Algorithms.Abstractions;
using Algorithms.Common;
using FluentResults;
using Org.BouncyCastle.Crypto;
using System.Security.Cryptography;

namespace Modelling.Models;
public sealed class Voter : BaseSecuredEntity
{
    public Guid Id { get; private set; }

    public string FullName { get; }

    public bool IsCapable { get; }

    public Guid RegistrationNumber { get; private set; }

    public Voter(string fullName, bool isCapable, Keys<DSAParameters> signatureKeys, Keys<AsymmetricKeyParameter> encryptionKeys, ISignatureProvider<DSAParameters> signatureProvider, IEncryptionProvider<AsymmetricKeyParameter> encryptionProvider, IObjectToByteArrayTransformer transformer)
        : base(signatureKeys, encryptionKeys, signatureProvider, encryptionProvider, transformer)
    {
        FullName = fullName;
        IsCapable = isCapable;
    }

    public void GenerateAndSetId()
    {
        Id = Guid.NewGuid();
    }

    public byte[] PrepareRegistrationNumberQuery(AsymmetricKeyParameter registrationBureauEncryptionPublicKey)
    {
        var signature = signatureProvider.Sign(transformer.Transform(FullName), signaturePrivateKey);
        var signedFullName = new SignedData<string>(FullName, signature);
        return encryptionProvider.Encrypt(transformer.Transform(signedFullName), registrationBureauEncryptionPublicKey);
    }

    public Result SetRegistrationNumber(byte[] encryptedRegistrationNumber, DSAParameters registrationBureauPublicKey)
    {
        return Result.Ok()
            .Bind(() => Result.Try(()
                => transformer.ReverseTransform<SignedData<Guid>>(encryptionProvider.Decrypt(encryptedRegistrationNumber, encryptionPrivateKey))
                    ?? throw new InvalidOperationException("Value cannot be transformed to signed ballot."),
                e => new Error("Message has wrong format or was incorrectly encrypted.").CausedBy(e)))
            .Bind(srn =>
            {
                var signatureIsAuthentic = signatureProvider.Verify(transformer.Transform(srn.Data), srn.Signature, registrationBureauPublicKey);
                if (!signatureIsAuthentic)
                {
                    return Result.Fail(new Error("The signature is not authentic."));
                }

                return Result.Ok(srn.Data);
            })
            .Bind(rn =>
            {
                RegistrationNumber = rn;
                return Result.Ok();
            });
    }

    public Result IsAbleToVote()
    {
        if (!IsCapable)
        {
            return Result.Fail($"Voter {FullName} is not capable.");
        }

        return Result.Ok();
    }
}
