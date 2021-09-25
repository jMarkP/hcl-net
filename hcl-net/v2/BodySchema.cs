namespace hcl_net.v2
{
    internal class BodySchema
    {
        public BodySchema(AttributeSchema[] attributes, BlockHeaderSchema[] blocks)
        {
            Attributes = attributes;
            Blocks = blocks;
        }

        public AttributeSchema[] Attributes { get; }
        public BlockHeaderSchema[] Blocks { get; }
    }
}