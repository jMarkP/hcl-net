namespace hcl_net.v2
{
    /// <summary>
    /// BlockHeaderSchema represents the shape of a block header, and is
    /// used for matching blocks within bodies.
    /// </summary>
    internal class BlockHeaderSchema
    {
        public BlockHeaderSchema(string type, string[] labelNames)
        {
            Type = type;
            LabelNames = labelNames;
        }

        public string Type { get; }
        public string[] LabelNames { get; }
    }
}