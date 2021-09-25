using System;

namespace hcl_net.v2
{
    internal readonly struct Range
    {
        private readonly byte[] _fileContents;

        public Range(string filename, byte[] fileContents, Pos start, Pos end)
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

        public Span<byte> GetBytes() => Length > 0 
            ? ((Span<byte>) _fileContents).Slice(Start.Byte, Length)
            : Span<byte>.Empty;

        public byte this[int index] => _fileContents[Start.Byte + index];
    }
}