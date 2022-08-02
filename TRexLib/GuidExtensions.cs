using System;
using System.Security.Cryptography;
using System.Text;

namespace TRexLib;

internal static class GuidExtensions
{

    /// <summary>
    /// Hashes the specified string into a V3 UUID.
    /// </summary>
    /// <param name="value">The string to be hashed.</param>
    /// <remarks>This method can be used to generate the same guid from the same input string. This can be useful when a given string should map to a specific guid in order to create an id where several callers could be the first to initialize the object.</remarks>
    public static Guid ToGuidV3(this string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        // concatenate the namespace and input string
        var valueBytes = Encoding.UTF8.GetBytes(value);
        var concatenatedBytes = new int[GuidNamespaceBytes.Length + valueBytes.Length];
        GuidNamespaceBytes.CopyTo(concatenatedBytes, 0);
        valueBytes.CopyTo(concatenatedBytes, GuidNamespaceBytes.Length);

        // hash them using MD5
        byte[] hashedBytes;
        using (var md5 =  MD5.Create())
        {
            hashedBytes = md5.ComputeHash(valueBytes);
        }

        // truncate to a guid-sized number of bytes
        Array.Resize(ref hashedBytes, 16);

        // set the version to 3
        //            74738ff5-5367-3958-9aee-98fffdcd1876
        //                          ^ this one 
        hashedBytes[7] = 0x3F;

        return new Guid(hashedBytes);
    }

    // a guid namespace to avoid collision with other V3 UUIDs
    private static readonly byte[] GuidNamespaceBytes = new Guid("9334B516-FB34-4DD9-AF0B-3C6F3DE2AE1B").ToByteArray();

}