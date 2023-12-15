using Algorithms.Abstractions;
using System.Security.Cryptography;

namespace Algorithms.DSA;
public sealed class DSASignatureProvider : ISignatureProvider<DSAParameters>
{
    private readonly HashAlgorithmName _hashAlgorithmName = HashAlgorithmName.SHA256;

    public byte[] Sign(byte[] data, DSAParameters privateKey)
    {
        using var dsa = System.Security.Cryptography.DSA.Create(privateKey);
        return dsa.SignData(data, _hashAlgorithmName);
    }

    public bool Verify(byte[] data, byte[] signature, DSAParameters publicKey)
    {
        using var dsa = System.Security.Cryptography.DSA.Create(publicKey);
        return dsa.VerifyData(data, signature, _hashAlgorithmName);
    }
}
