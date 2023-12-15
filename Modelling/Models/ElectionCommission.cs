using Algorithms.Abstractions;
using Algorithms.Common;
using Org.BouncyCastle.Crypto;
using System.Security.Cryptography;

namespace Modelling.Models;
public sealed class ElectionCommission : BaseSecuredEntity
{
    private readonly Dictionary<Guid, DSAParameters> _voters;

    public ElectionCommission(Dictionary<Guid, DSAParameters> voters, Keys<DSAParameters> signatureKeys, Keys<AsymmetricKeyParameter> encryptionKeys, ISignatureProvider<DSAParameters> signatureProvider, IEncryptionProvider<AsymmetricKeyParameter> encryptionProvider, IObjectToByteArrayTransformer transformer)
        : base(signatureKeys, encryptionKeys, signatureProvider, encryptionProvider, transformer)
    {
        _voters = voters;
    }
}
