using Algorithms.Abstractions;
using Algorithms.Common;
using Modelling.Models;
using Org.BouncyCastle.Crypto;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace Demo;
public sealed class DemoDataFactory
{
    private readonly IEncryptionProvider<AsymmetricKeyParameter> _encryptionProvider;
    private readonly IKeyGenerator<AsymmetricKeyParameter> _encryptionKeyGenerator;

    private readonly ISignatureProvider<DSAParameters> _signatureProvider;
    private readonly IKeyGenerator<DSAParameters> _signatureKeyGenerator;

    private readonly IObjectToByteArrayTransformer _transformer;

    public DemoDataFactory(IEncryptionProvider<AsymmetricKeyParameter> encryptionProvider, IKeyGenerator<AsymmetricKeyParameter> encryptionKeyGenerator, ISignatureProvider<DSAParameters> signatureProvider, IKeyGenerator<DSAParameters> keysignatureGenerator, IObjectToByteArrayTransformer objectToByteArrayTransformer)
    {
        _encryptionProvider = encryptionProvider;
        _encryptionKeyGenerator = encryptionKeyGenerator;
        _signatureProvider = signatureProvider;
        _signatureKeyGenerator = keysignatureGenerator;
        _transformer = objectToByteArrayTransformer;
    }

    public IReadOnlyList<Candidate> CreateCandidates()
    {
        return new List<Candidate>
        {
            new (1, "Ishaan Allison"),
            new (2, "Oliver Mendez"),
            new (3, "Naomi Winter"),
        };
    }

    public IReadOnlyList<Voter> CreateVoters()
    {
        var keys = new ConcurrentBag<(Keys<DSAParameters>, Keys<AsymmetricKeyParameter>)>();
        Parallel.For(0, 8, _ =>
        {
            keys.Add((_signatureKeyGenerator.Generate(), _encryptionKeyGenerator.Generate()));
        });

        var votersInfo = new List<(string, bool)>
        {
            ("Jasper Lambert", true),
            ("Jonty Levine", false),
            ("Nathaniel Middleton", true),
            ("Nathan Bass", true),
            ("Aran Doyle", true),
            ("Julian Harper", true),
            ("Lucian Gross", true),

            ("Alicia Sierra", true)
        };

        return votersInfo.Zip(keys)
            .Select(info => new Voter(info.First.Item1, info.First.Item2, info.Second.Item1, info.Second.Item2, _signatureProvider, _encryptionProvider, _transformer))
            .ToList();

        //return new List<Voter>
        //{
        //    new ("Jasper Lambert", true, _signatureKeyGenerator.Generate(), _encryptionKeyGenerator.Generate(), _signatureProvider, _encryptionProvider, _transformer),
        //    new ("Jonty Levine", false, _signatureKeyGenerator.Generate(), _encryptionKeyGenerator.Generate(), _signatureProvider, _encryptionProvider, _transformer),      // Not capable.
        //    new ("Nathaniel Middleton", true, _signatureKeyGenerator.Generate(), _encryptionKeyGenerator.Generate(), _signatureProvider, _encryptionProvider, _transformer),
        //    new ("Nathan Bass", true, _signatureKeyGenerator.Generate(), _encryptionKeyGenerator.Generate(), _signatureProvider, _encryptionProvider, _transformer),
        //    new ("Aran Doyle", true, _signatureKeyGenerator.Generate(), _encryptionKeyGenerator.Generate(), _signatureProvider, _encryptionProvider, _transformer),
        //    new ("Julian Harper", true, _signatureKeyGenerator.Generate(), _encryptionKeyGenerator.Generate(), _signatureProvider, _encryptionProvider, _transformer),
        //    new ("Lucian Gross", true, _signatureKeyGenerator.Generate(), _encryptionKeyGenerator.Generate(), _signatureProvider, _encryptionProvider, _transformer),

        //    new ("Alicia Sierra", true, _signatureKeyGenerator.Generate(), _encryptionKeyGenerator.Generate(), _signatureProvider, _encryptionProvider, _transformer)
        //};
    }

    public RegistrationBureau CreateRegistrationBureau(IEnumerable<Voter> voters)
    {
        return new RegistrationBureau(voters, _signatureKeyGenerator.Generate(), _encryptionKeyGenerator.Generate(), _signatureProvider, _encryptionProvider, _transformer);
    }

    public Dictionary<Voter, int> CreateVotersWithCandidateIds(IReadOnlyList<Voter> voters)
    {
        var dictionary = new Dictionary<Voter, int>();
        for (var i = 0; i < voters.Count; i++)
        {
            var candidateId = (i % 7 + 1) switch
            {
                1 => 1,
                2 => 1,

                3 => 2,
                4 => 1,
                5 => 3,
                6 => 3,
                7 => 3,

                _ => throw new InvalidOperationException("Negative and zero voters' ids are not supported in this method.")
            };
            dictionary.Add(voters[i], candidateId);
        }
        return dictionary;
    }
}
