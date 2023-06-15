using System.Security.Cryptography;

namespace Urdep.Extensions.Data;

/// <summary>
/// Hashing helper.
/// </summary>
public static class Hashing
{
    /// <summary>
    /// Gets a hash algorithm with the given name.
    /// </summary>
    /// <param name="algo">Which algorithm to get.</param>
    /// <returns>The hash function.</returns>
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
