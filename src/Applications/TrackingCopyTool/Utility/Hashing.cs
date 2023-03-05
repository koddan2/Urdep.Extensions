using System.Security.Cryptography;

namespace TrackingCopyTool.Utility;

/// <summary>
/// Helper class for hashing operations.
/// </summary>
internal static class Hashing
{
    /// <summary>
    /// Gets an instance of a specific hash algo.
    /// </summary>
    /// <param name="algo">The algo to instatiate.</param>
    /// <returns>The instance.</returns>
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
