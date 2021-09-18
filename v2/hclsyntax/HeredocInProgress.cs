namespace hcl_net.v2.hclsyntax
{
    internal readonly struct HeredocInProgress
    {
        public HeredocInProgress(byte[] marker, bool startOfLine)
        {
            Marker = marker;
            StartOfLine = startOfLine;
        }

        public byte[] Marker { get; }
        public bool StartOfLine { get; }
    }
}