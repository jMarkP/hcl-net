namespace hcl_net.v2.hclsyntax
{
    internal class HeredocInProgress
    {
        public HeredocInProgress(byte[] marker, bool startOfLine)
        {
            Marker = marker;
            StartOfLine = startOfLine;
        }

        public byte[] Marker { get; }
        public bool StartOfLine { get; set; }
    }
}