using System;
using System.Text;
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

        public string String => GetBytes().Length > 0 ? Encoding.UTF8.GetString(GetBytes()) : "";

        public override string ToString()
        {
            return $"[{Type}|{Range.Start.Byte}:{Range.End.Byte}] '{String}'";
        }
    }
}