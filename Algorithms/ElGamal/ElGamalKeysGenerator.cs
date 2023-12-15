using Algorithms.Abstractions;
using Algorithms.Common;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Algorithms.ElGamal;
public sealed class ElGamalKeysGenerator : IKeyGenerator<AsymmetricKeyParameter>
{
    private const int _keySize = 1024;

    public Keys<AsymmetricKeyParameter> Generate()
    {
        var parGen = new ElGamalParametersGenerator();

        parGen.Init(_keySize, 10, new SecureRandom());
        var elParams = parGen.GenerateParameters();

        // Generate keypair
        var _pGen = new ElGamalKeyPairGenerator();

        _pGen.Init(new ElGamalKeyGenerationParameters(new SecureRandom(), elParams));

        var pair = _pGen.GenerateKeyPair();

        return new Keys<AsymmetricKeyParameter>(pair.Public, pair.Private);
    }
}
