using System;
using System.Text;
using hcl_net.Utilities;

namespace hcl_net.v2.hclsyntax
{
    internal readonly struct Token
    {
        private readonly byte[] _fileContents;
        public Token(TokenType type, Range range, byte[] fileContents)
        {
            _fileContents = fileContents;
            Type = type;
            Range = range;
        }

        public TokenType Type { get; }
        public Range Range { get; }

        public Span<byte> GetBytes() => Range.Length > 0 
            ? ((Span<byte>) _fileContents).Slice(Range.Start.Byte, Range.Length)
            : Span<byte>.Empty;

        public byte this[int index] => _fileContents[Range.Start.Byte + index];

        public string String => GetBytes().Length > 0 ? Encoding.UTF8.GetString(GetBytes()) : "";

        public override string ToString()
        {
            return $"[{Type}|{Range.Start.Byte}:{Range.End.Byte}] '{String}'";
        }
    }
}