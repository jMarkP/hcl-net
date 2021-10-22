using System.Collections.Generic;

namespace hcl_net.v2
{
    internal class BodyContent
    {
        public BodyContent(Attributes attributes, Blocks blocks, Range missingItemRange)
        {
            Attributes = attributes;
            Blocks = blocks;
            MissingItemRange = missingItemRange;
        }

        public Attributes Attributes { get; }
        public Blocks Blocks { get; }
        public Range MissingItemRange { get; }
    }
}