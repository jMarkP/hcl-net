namespace hcl_net.Parser.HCL
{
    internal enum TokenType
    {
        ILLEGAL,

        EOF,
        COMMENT,

        identifier_begin,
        IDENT,
        literal_begin,
        NUMBER,
        FLOAT,
        BOOL,
        STRING,
        literal_end,
        identifier_end,

        operator_begin,
        LBRACK,
        LBRACE,
        COMMA,
        PERIOD,
        HEREDOC,

        RBRACK,
        RBRACE,

        ASSIGN,
        ADD,
        SUB,
        operator_end
    }

    internal static class TokenTypeExtensions
    {
        public static bool IsIdentifier(this TokenType t)
        {
            return TokenType.identifier_begin < t && t < TokenType.identifier_end;
        }

        public static bool IsLiteral(this TokenType t)
        {
            return TokenType.literal_begin < t && t < TokenType.literal_end;
        }

        public static bool IsOperator(this TokenType t)
        {
            return TokenType.operator_begin < t && t < TokenType.operator_end;
        }
    }
}