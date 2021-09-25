using System.Collections.Generic;

namespace hcl_net.v2.hclsyntax
{
    internal class TokenAccum
    {
        public TokenAccum(string filename, byte[] bytes, Pos pos, int startByte)
        {
            Filename = filename;
            Bytes = bytes;
            Pos = pos;
            StartByte = startByte;
        }

        public string Filename { get; }
        public byte[] Bytes { get; }
        public Pos Pos { get; set; }
        public List<Token> Tokens { get; } = new List<Token>();

        public int StartByte { get; }

        public void EmitToken(TokenType tokenType, int startOffset, int endOffset)
        {
            var start = Pos.AdvancedToOffsetIn(Bytes, startOffset);
            var end = start.AdvancedToOffsetIn(Bytes, endOffset);
            Pos = end;
            var token = new Token(tokenType, new Range(Filename, Bytes, start, end));
            Tokens.Add(token);
        }
    }
}