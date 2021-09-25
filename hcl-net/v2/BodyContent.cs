using System.Collections.Generic;

namespace hcl_net.v2
{
    internal class BodyContent
    {
        public BodyContent(IDictionary<string, Attribute> attributes, Block[] blocks, Range missingItemRange)
        {
            Attributes = attributes;
            Blocks = blocks;
            MissingItemRange = missingItemRange;
        }

        public IDictionary<string, Attribute> Attributes { get; }
        public Block[] Blocks { get; }
        public Range MissingItemRange { get; }
    }
}