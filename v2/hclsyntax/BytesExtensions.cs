using System.Text;

namespace hcl_net.v2.hclsyntax
{
    internal static class BytesExtensions
    {
        // Implements Go's bytes.TrimSpace in C#
        // Naive implementation for now that roundtrips
        // to a string
        // TODO: Implement this with 0 allocations
        public static byte[] TrimSpace(this byte[] s)
        {
            var asString = Encoding.UTF8.GetString(s);
            asString = asString.Trim();
            return Encoding.UTF8.GetBytes(asString);
        }
    }
}