using System;
using hcl_net.Utilities;

namespace hcl_net.v2.hclsyntax
{
    internal readonly struct Token
    {
        public Token(TokenType type, TokenRange range)
        {
            Type = type;
            Range = range;
        }

        public TokenType Type { get; }
        public TokenRange Range { get; }

        public Span<byte> GetBytes() => Range.GetBytes();

        public byte this[int index] => Range[index];
    }
}