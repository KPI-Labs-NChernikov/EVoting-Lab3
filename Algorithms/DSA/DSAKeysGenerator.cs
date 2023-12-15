using Algorithms.Abstractions;
using Algorithms.Common;
using System.Security.Cryptography;

namespace Algorithms.DSA;
public sealed class DSAKeysGenerator : IKeyGenerator<DSAParameters>
{
    public Keys<DSAParameters> Generate()
    {
        using var dsa = System.Security.Cryptography.DSA.Create();
        return new Keys<DSAParameters>(dsa.ExportParameters(false), dsa.ExportParameters(true));
    }
}
