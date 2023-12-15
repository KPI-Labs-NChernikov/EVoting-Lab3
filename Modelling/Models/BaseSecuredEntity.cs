using Algorithms.Abstractions;
using Algorithms.Common;
using Org.BouncyCastle.Crypto;
using System.Security.Cryptography;

namespace Modelling.Models;
public abstract class BaseSecuredEntity
{
    public DSAParameters SignaturePublicKey { get; }
    protected readonly DSAParameters signaturePrivateKey;

    public AsymmetricKeyParameter EncryptionPublicKey { get; }
    protected readonly AsymmetricKeyParameter encryptionPrivateKey;

    protected readonly ISignatureProvider<DSAParameters> signatureProvider;
    protected readonly IEncryptionProvider<AsymmetricKeyParameter> encryptionProvider;

    protected readonly IObjectToByteArrayTransformer transformer;

    public BaseSecuredEntity(Keys<DSAParameters> signatureKeys, Keys<AsymmetricKeyParameter> encryptionKeys, ISignatureProvider<DSAParameters> signatureProvider, IEncryptionProvider<AsymmetricKeyParameter> encryptionProvider, IObjectToByteArrayTransformer transformer)
    {
        SignaturePublicKey = signatureKeys.PublicKey;
        signaturePrivateKey = signatureKeys.PrivateKey;

        EncryptionPublicKey = encryptionKeys.PublicKey;
        encryptionPrivateKey = encryptionKeys.PrivateKey;

        this.signatureProvider = signatureProvider;
        this.encryptionProvider = encryptionProvider;
        this.transformer = transformer;
    }
}
