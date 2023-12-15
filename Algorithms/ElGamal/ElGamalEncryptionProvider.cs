using Algorithms.Abstractions;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;

namespace Algorithms.ElGamal;
public sealed class ElGamalEncryptionProvider : IEncryptionProvider<AsymmetricKeyParameter>
{
    public byte[] Decrypt(byte[] data, AsymmetricKeyParameter privateKey)
    {
        ElGamalEngine decryptEng = new ElGamalEngine();
        decryptEng.Init(false, privateKey);
        return decryptEng.ProcessBlock(data, 0, data.Length);
    }

    public byte[] Encrypt(byte[] data, AsymmetricKeyParameter publicKey)
    {
        ElGamalEngine cryptEng = new ElGamalEngine();

        cryptEng.Init(true, publicKey);

        return cryptEng.ProcessBlock(data, 0, data.Length);
    }
}
