using System;

namespace hcl_net.v2.hclsyntax
{
    internal readonly struct TokenRange
    {
        private readonly byte[] _fileContents;

        public TokenRange(string filename, byte[] fileContents, Pos start, Pos end)
        {
            _fileContents = fileContents;
            Filename = filename;
            Start = start;
            End = end;
            Length = end.Byte - start.Byte;
        }

        public string Filename { get; }
        public Pos Start { get; }
        public Pos End { get; }
        public int Length { get; }

        public Span<byte> GetBytes() => ((Span<byte>) _fileContents).Slice(Start.Byte, Length);

        public byte this[int index] => _fileContents[Start.Byte + index];
    }
}