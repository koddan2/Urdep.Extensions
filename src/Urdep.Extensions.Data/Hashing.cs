using System.Security.Cryptography;

namespace Urdep.Extensions.Data;

public static class Hashing
{
    public static HashAlgorithm GetHashAlgorithmInstance(HashAlgo algo)
    {
        return algo switch
        {
            HashAlgo.MD5 => MD5.Create(),
            HashAlgo.SHA1 => SHA1.Create(),
            HashAlgo.SHA256 => SHA256.Create(),
            HashAlgo.SHA384 => SHA384.Create(),
            HashAlgo.SHA512 => SHA512.Create(),
        };
    }
}
