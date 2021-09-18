namespace hcl_net.v2.hclsyntax
{
    internal enum TokenType
	{
		// Single-character tokens are represented by their own character, for
		// convenience in producing these within the scanner. However, the values
		// are otherwise arbitrary and just intended to be mnemonic for humans
		// who might see them in debug output.

		TokenOBrace = '{',
		TokenCBrace = '}',
		TokenOBrack = '[',
		TokenCBrack = ']',
		TokenOParen = '(',
		TokenCParen = ')',
		TokenOQuote = '«',
		TokenCQuote = '»',
		TokenOHeredoc = 'H',
		TokenCHeredoc = 'h',

		TokenStar = '*',
		TokenSlash = '/',
		TokenPlus = '+',
		TokenMinus = '-',
		TokenPercent = '%',

		TokenEqual = '=',
		TokenEqualOp = '≔',
		TokenNotEqual = '≠',
		TokenLessThan = '<',
		TokenLessThanEq = '≤',
		TokenGreaterThan = '>',
		TokenGreaterThanEq = '≥',

		TokenAnd = '∧',
		TokenOr = '∨',
		TokenBang = '!',

		TokenDot = '.',
		TokenComma = ',',

		TokenEllipsis = '…',
		TokenFatArrow = '⇒',

		TokenQuestion = '?',
		TokenColon = ':',

		TokenTemplateInterp = '∫',
		TokenTemplateControl = 'λ',
		TokenTemplateSeqEnd = '∎',

		TokenQuotedLit = 'Q', // might contain backslash escapes
		TokenStringLit = 'S', // cannot contain backslash escapes
		TokenNumberLit = 'N',
		TokenIdent = 'I',

		TokenComment = 'C',

		TokenNewline = '\n',
		TokenEOF = '␄',

		// The rest are not used in the language but recognized by the scanner so
		// we can generate good diagnostics in the parser when users try to write
		// things that might work in other languages they are familiar with, or
		// simply make incorrect assumptions about the HCL language.

		TokenBitwiseAnd = '&',
		TokenBitwiseOr = '|',
		TokenBitwiseNot = '~',
		TokenBitwiseXor = '^',
		TokenStarStar = '➚',
		TokenApostrophe = '\'',
		TokenBacktick = '`',
		TokenSemicolon = ';',
		TokenTabs = '␉',
		TokenInvalid = '�',
		TokenBadUTF8 = '#',
		TokenQuotedNewline = '␤',

		// TokenNil is a placeholder for when a token is required but none is
		// available, e.g. when reporting errors. The scanner will never produce
		// this as part of a token stream.
		TokenNil = '\x00',
	};
}