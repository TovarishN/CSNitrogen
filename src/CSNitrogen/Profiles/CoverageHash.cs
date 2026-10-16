using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
namespace CSNitrogen.Profiles;
public static class CoverageHash
{
    public static string Compute(IEnumerable<KeyValuePair<string,byte[]>> inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        var sorted=new SortedDictionary<string,byte[]>(StringComparer.Ordinal);
        foreach(var input in inputs)
        {
            ArgumentException.ThrowIfNullOrEmpty(input.Key);ArgumentNullException.ThrowIfNull(input.Value);
            if(!sorted.TryAdd(input.Key.Replace('\\','/'),input.Value))throw new ArgumentException("Duplicate coverage input name.",nameof(inputs));
        }
        using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<byte> length=stackalloc byte[8];
        foreach(var input in sorted)
        {
            byte[] name=Encoding.UTF8.GetBytes(input.Key);
            BinaryPrimitives.WriteInt64LittleEndian(length,name.Length);hash.AppendData(length);hash.AppendData(name);
            BinaryPrimitives.WriteInt64LittleEndian(length,input.Value.LongLength);hash.AppendData(length);hash.AppendData(input.Value);
        }
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }
}
