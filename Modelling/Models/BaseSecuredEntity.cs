using Algorithms.Common;
using Org.BouncyCastle.Crypto;
using System.Security.Cryptography;

namespace Modelling.Models;
public abstract class BaseSecuredEntity
{
    public DSAParameters SignaturePublicKey { get; }
    protected readonly DSAParameters _signaturePrivateKey;

    public AsymmetricKeyParameter EncryptionPublicKey { get; }
    protected readonly AsymmetricKeyParameter _encryptionPrivateKey;

    public BaseSecuredEntity(Keys<DSAParameters> signatureKeys, Keys<AsymmetricKeyParameter> encryptionKeys)
    {
        SignaturePublicKey = signatureKeys.PublicKey;
        _signaturePrivateKey = signatureKeys.PrivateKey;

        EncryptionPublicKey = encryptionKeys.PublicKey;
        _encryptionPrivateKey = encryptionKeys.PrivateKey;
    }
}
