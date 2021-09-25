namespace hcl_net.v2
{
    /// <summary>
    /// File is the top-level node that results from parsing a HCL file.
    /// </summary>
    internal class File
    {
        public File(IBody body, byte[] bytes, object nav)
        {
            Body = body;
            Bytes = bytes;
            Nav = nav;
        }

        public IBody Body { get; }
        public byte[] Bytes { get; }
        
        // Nav is used to integrate with the "hcled" editor integration package,
        // and with diagnostic information formatters. It is not for direct use
        // by a calling application.
        public object Nav { get; }
    }
}