using System.Collections.Generic;
using System.Linq;

namespace hcl_net.v2
{
    internal static class BlocksExtensions
    {
        public static IEnumerable<Block> OfType(this IEnumerable<Block> els, string typeName)
        {
            return els.Where(el => el.Type.Equals(typeName));
        }
        public static ILookup<string, Block> ByType(this IEnumerable<Block> els)
        {
            return els.ToLookup(el => el.Type);
        }
    }
}