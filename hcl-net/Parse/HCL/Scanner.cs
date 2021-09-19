using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace hcl_net.Parse.HCL
{
    class Scanner
    {
        private const char Eof = (char)0;

        /// <summary>
        /// Source buffer for immutable access
        /// </summary>
        private readonly string _buffer;

        /// <summary>
        /// Current position in the source
        /// </summary>
        private Pos srcPos;

        /// <summary>
        /// Previous position - used for peek()
        /// </summary>
        private Pos prevPos;

        /// <summary>
        /// Token text start position
        /// </summary>
        private int tokenStart;

        /// <summary>
        /// Token text end position
        /// </summary>
        private int tokenEnd;

        private int errorCount;

        public delegate void ErrorDelegate(Pos pos, string message);
        /// <summary>
        /// Error is called for each error encountered.
        /// If not set will report to STDERR
        /// </summary>
        public event ErrorDelegate Error;

        /// <summary>
        /// The start position of the most recently scanned token.
        /// Set by Scan(). The Filename field is always left untouched
        /// by the scanner. If an error is reported (via Error) and this
        /// is invalid, the scanner is not inside a token
        /// </summary>
        private Pos tokenPos;

        public Scanner(string buffer)
            : this(buffer, null)
        {
        }

        public Scanner(string buffer, string filename)
        {
            _buffer = buffer;
            srcPos = Pos.CreateForFile(filename);
        }

        public int ErrorCount
        {
            get { return errorCount; }
        }

        #region Move to Pos
        public char Next()
        {
            char c;
            prevPos = srcPos;
            srcPos = srcPos.NextInString(_buffer, out c);
            return c;
        }

        public void Unread()
        {
            if (srcPos.Offset == 0)
                throw new InvalidOperationException("Cannot unread past beginning of string");
            srcPos = prevPos;
        }

        public char Peek()
        {
            return srcPos.PeekInString(_buffer);
        }
        #endregion

        public Token Scan()
        {
            var ch = Next();

            while (IsWhitespace(ch))
            {
                ch = Next();
            }

            tokenStart = prevPos.Offset;
            tokenPos = prevPos;

            TokenType tokenType;
            string tokenText;
            if (IsLetter(ch))
            {
                ScanIdentifier(ch, out tokenType);
            }
            else if (IsDecimal(ch))
            {
                ScanNumber(ch, out tokenType);
            }
            else
            {
                switch (ch)
                {
                    case Eof:
                        tokenType = TokenType.EOF;
                        // Short circuit EOF
                        return new Token(tokenType, tokenPos, "", false);
                    case '"':
                        tokenType = TokenType.STRING;
                        ScanString(ch);
                        break;
                    case '#':
                    case '/':
                        tokenType = TokenType.COMMENT;
                        ScanComment(ch);
                        break;
                    case '.':
                        tokenType = TokenType.PERIOD;
                        ch = Peek();
                        if (IsDecimal(ch))
                        {
                            tokenType = TokenType.FLOAT;
                            ch = ScanMantissa(ch);
                            ch = ScanExponent(ch);
                        }
                        break;
                    case '<':
                        tokenType = TokenType.HEREDOC;
                        ScanHeredoc(ch);
                        break;
                    case '[':
                        tokenType = TokenType.LBRACK;
                        break;
                    case ']':
                        tokenType = TokenType.RBRACK;
                        break;
                    case '{':
                        tokenType = TokenType.LBRACE;
                        break;
                    case '}':
                        tokenType = TokenType.RBRACE;
                        break;
                    case ',':
                        tokenType = TokenType.COMMA;
                        break;
                    case '=':
                        tokenType = TokenType.ASSIGN;
                        break;
                    case '+':
                        tokenType = TokenType.ADD;
                        break;
                    case '-':
                        if (IsDecimal(Peek()))
                        {
                            ch = Next();
                            ScanNumber(ch, out tokenType);
                        }
                        else
                        {
                            tokenType = TokenType.SUB;
                        }
                        break;
                    default:
                        Err("Illegal char");
                        return null;
                }
            }

            tokenEnd = Math.Min(srcPos.Offset, _buffer.Length);
            var len = tokenEnd - tokenStart;
            tokenText = _buffer.Substring(tokenPos.Offset, len);

            return new Token(tokenType, tokenPos, tokenText, false);
        }

        private void ScanComment(char ch)
        {
            // Single line comments
            if (ch == '#' || (ch == '/' && Peek() != '*'))
            {
                // Single line comments must either start with '#'
                // or '//'
                if (ch == '/' && Peek() != '/')
                {
                    Err("Expected '/' for comment");
                    return;
                }
                ch = Next();
                while (ch != '\n' && ch != Eof)
                {
                    ch = Next();
                }
                if (ch != Eof)
                {
                    Unread();
                }
                return;
            }

            if (ch == '/')
            {
                ch = Next();
                ch = Next(); // Read the character after /*
            }
            // Repeat until we read */ or reach the end of the file
            while (true)
            {
                if (ch == Eof)
                {
                    Err("Comment not terminated");
                    break;
                }
                var ch0 = ch;
                ch = Next();
                if (ch0 == '*' && ch == '/')
                {
                    break;
                }
            }
        }

        private void ScanNumber(char ch, out TokenType tokenType)
        {
            if (ch == '0')
            {
                // Check for hexadecimal, octal, or float
                ch = Next();
                if (ch == 'x' || ch == 'X')
                {
                    ch = Next();
                    var found = false;
                    while (IsHexadecimal(ch))
                    {
                        ch = Next();
                        found = true;
                    }
                    if (!found)
                    {
                        Err("Illegal Hexadecimal number");
                    }
                    if (ch != Eof)
                    {
                        Unread();
                    }
                    tokenType = TokenType.NUMBER;
                    return;
                }

                // Now we know it's either 0421 (i.e. octal) or 0.1212 (float)
                var illegalOctal = false;
                while (IsDecimal(ch))
                {
                    ch = Next();
                    if (ch == '8' || ch == '9')
                    {
                        // this is just a possibility. For example 0159 is illegal, but
                        // 0159.23 is valid. So we mark a possible illegal octal. If
                        // the next character is not a period, we'll print the error.
                        illegalOctal = true;
                    }
                }
                if (ch == 'e' || ch == 'E')
                {
                    ch = ScanExponent(ch);
                    tokenType = TokenType.FLOAT;
                    return;
                }
                if (ch == '.')
                {
                    ch = ScanFraction(ch);
                    if (ch == 'e' || ch == 'E')
                    {
                        ch = Next();
                        ch = ScanExponent(ch);
                    }
                    tokenType = TokenType.FLOAT;
                    return;
                }

                if (illegalOctal)
                {
                    Err("Illegal octal number");
                    tokenType = TokenType.NUMBER;
                    return;
                }
                if (ch != Eof)
                {
                    Unread();
                }
                tokenType = TokenType.NUMBER;
                return;
            }
            // Didn't start with a 0, so it's a base-10
            //  number or float
            ScanMantissa(ch);
            ch = Next();
            if (ch == 'e' || ch == 'E')
            {
                ch = ScanExponent(ch);
                tokenType = TokenType.FLOAT;
                return;
            }
            if (ch == '.')
            {
                ch = ScanFraction(ch);
                if (ch == 'e' || ch == 'E')
                {
                    ch = Next();
                    ch = ScanExponent(ch);
                }
                tokenType = TokenType.FLOAT;
                return;
            }

            if (ch != Eof)
            {
                Unread();
            }
            tokenType = TokenType.NUMBER;
        }


        /// <summary>
        /// Scan the mantissa of a number.
        /// Returns the next char to be used to determine
        /// whether this is part of a fraction or an exponent
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private char ScanMantissa(char ch)
        {
            var scanned = false;
            while (IsDecimal(ch))
            {
                ch = Next();
                scanned = true;
            }
            if (scanned && ch != Eof)
            {
                Unread();
            }
            return ch;
        }

        /// <summary>
        /// Scan the fractional part of a number if it has one
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private char ScanFraction(char ch)
        {
            if (ch == '.')
            {
                ch = Peek();
                ch = ScanMantissa(ch);
            }
            return ch;
        }

        private char ScanExponent(char ch)
        {
            if (ch == 'e' || ch == 'E')
            {
                ch = Next();
                if (ch == '-' || ch == '+')
                {
                    ch = Next();
                }
                ch = ScanMantissa(ch);
            }
            return ch;
        }

        private void ScanHeredoc(char ch)
        {
            // Scan the second '<' in example '<<EOF'
            if (Next() != '<')
            {
                Err("heredoc expected second '<', didn't see it");
                return;
            }

            var offs = srcPos.Offset;
            ch = Next();
            // - after the << indicates the heredoc indentation should
            //  be removed
            if (ch == '-')
            {
                ch = Next();
                offs++;
            }

            // Read the 'EOF' anchor
            while (IsLetter(ch) || IsDigit(ch))
            {
                ch = Next();
            }
            // Check for EOF
            if (ch == Eof)
            {
                Err("Heredoc not terminated");
                return;
            }

            // Ignore \r in Windows line endings
            if (ch == '\r')
            {
                if (Peek() == '\n')
                {
                    ch = Next();
                }
            }

            if (ch != '\n')
            {
                Err("Invalid characters in heredoc anchor");
                return;
            }

            var identifier = _buffer.Substring(offs, srcPos.Offset - offs - 1);
            if (identifier.Length == 0)
            {
                Err("Zero-length heredoc anchor");
                return;
            }

            var regex = new Regex(string.Format(@"^\s*{0}", identifier));

            var lineStart = srcPos.Offset;
            while (true)
            {
                // Keep reading until we find a newline
                ch = Next();
                if (ch == '\n')
                {
                    // First check if the line is at least long enough
                    //  to match the identifier
                    var lineLength = srcPos.Offset - lineStart;
                    if (lineLength > identifier.Length
                        && regex.IsMatch(_buffer.Substring(lineStart, lineLength)))
                    {
                        break;
                    }
                    lineStart = srcPos.Offset;
                }
                if (ch == Eof)
                {
                    Err("heredoc not terminated");
                    return;
                }
            }
        }

        private void ScanString(char ch)
        {
            var braces = 0;
            while (true)
            {
                // '"' opening already consumed
                // read character after quote
                ch = Next();

                if (ch == Eof)
                {
                    Err("Literal not terminated");
                    return;
                }
                if (ch == '"' && braces == 0)
                {
                    break;
                }
                // if we're going into a ${} then we can ignore quotes
                // for a while
                if (braces == 0 && ch == '$' && Peek() == '{')
                {
                    braces++;
                    // Consume the '{'
                    Next();
                }
                else if (braces > 0 && ch == '{')
                {
                    braces++;
                }
                if (braces > 0 && ch == '}')
                {
                    braces--;
                }

                if (ch == '\\')
                {
                    ScanEscape();
                }
            }
        }

        private void ScanEscape()
        {
            // Read the character after '/'
            var ch = Next();
            switch (ch)
            {
                case 'a':
                case 'b':
                case 'f':
                case 'n':
                case 'r':
                case 't':
                case 'v':
                case '\\':
                case '"':
                    return;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                    // Octal
                    ScanDigits(ch, 8, 3);
                    return;
                case 'x':
                    // Hex - 2 bytes
                    ScanDigits(Next(), 16, 2);
                    return;
                case 'u':
                    // Hex - 4 bytes
                    ScanDigits(Next(), 16, 4);
                    return;
                case 'U':
                    // Hex - 8 bytes
                    ScanDigits(Next(), 16, 8);
                    return;
                default:
                    Err("Illegal char escape");
                    return;
            }
        }

        private void ScanDigits(char ch, int numberBase, int n)
        {
            var start = n;
            // Scan at most n digits
            // Stop once we scan n, reach Eof, or read a character
            //  that doesn't fit in numberBase
            while (n > 0 && DigitVal(ch) < numberBase)
            {
                ch = Next();
                if (ch == Eof)
                {
                    break;
                }
                n--;
            }
            if (n > 0)
            {
                Err("Illegal char escape");
            }
            if (n != start)
            {
                // We scanned all digits, put the last (i.e. non-digit)
                // char back (only if we read anything at all)
                Unread();
            }
        }

        private void ScanIdentifier(char ch, out TokenType tokenType)
        {
            tokenType = TokenType.IDENT;
            var offs = prevPos.Offset;
            ch = Next();
            while (IsLetter(ch)
                   || IsDigit(ch)
                   || ch == '-'
                   || ch == '.')
            {
                ch = Next();
            }

            Unread();

            // We need to examine the identifier to see if it's
            //  a reserved word
            var identifier = _buffer.Substring(offs, srcPos.Offset - offs);
            // BOOL constants are currently the only reserved words
            if (identifier == "true" || identifier == "false")
            {
                tokenType = TokenType.BOOL;
            }
        }

        private static readonly char[] whitespaceChars =
        {
            ' ', '\t', '\n', '\r'
        };
        private bool IsWhitespace(char ch)
        {
            return whitespaceChars.Contains(ch);
        }

        private bool IsDecimal(char ch)
        {
            return ('0' <= ch && ch <= '9');
        }

        private bool IsHexadecimal(char ch)
        {
            return ('0' <= ch && ch <= '9')
                   || ('a' <= ch && ch <= 'f')
                   || ('A' <= ch && ch <= 'F');
        }

        private void Err(string message)
        {
            errorCount++;
            var pos = prevPos;
            if (Error != null)
            {
                Error(pos, message);
            }
            else
            {
                Console.Error.WriteLine("{0}: {1}", pos, message);
            }
        }

        private bool IsLetter(char ch)
        {
            return ('a' <= ch && ch <= 'z')
                   || ('A' <= ch && ch <= 'Z')
                   || ch == '_'
                   || (ch >= 0x80 && char.IsLetter(ch));
        }

        private bool IsDigit(char ch)
        {
            return IsDecimal(ch) || (ch >= 0x80 && char.IsDigit(ch));
        }

        /// <summary>
        /// Get the numeric value of a digit (0-F/f)
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private int DigitVal(char ch)
        {
            if ('0' <= ch && ch <= '9')
                return ch - '0';
            if ('a' <= ch && ch <= 'f')
                return ch - 'a' + 10;
            if ('A' <= ch && ch <= 'F')
                return ch - 'A' + 10;
            return 16; // Larger than any legitimate digit
        }
    }
}
