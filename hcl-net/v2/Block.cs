namespace hcl_net.v2
{
    /// <summary>
    /// Block represents a nested block within a Body.
    /// </summary>
    internal class Block
    {
        public Block(string type, string[] labels, IBody body, Range defRange, Range typeRange, Range[] labelRanges)
        {
            Type = type;
            Labels = labels;
            Body = body;
            DefRange = defRange;
            TypeRange = typeRange;
            LabelRanges = labelRanges;
        }

        public string Type { get; }
        public string[] Labels { get; }
        public IBody Body { get; }
        
        /// <summary>
        /// Range that can be considered the "definition" for seeking in an editor
        /// </summary>
        public Range DefRange { get; }
        /// <summary>
        /// Range for the block type declaration specifically.
        /// </summary>
        public Range TypeRange { get; }
        /// <summary>
        /// Ranges for the label values specifically.
        /// </summary>
        public Range[] LabelRanges { get; }
    }
}