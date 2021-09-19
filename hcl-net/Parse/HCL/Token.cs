using System;
using System.Linq;
using hcl_net.Utilities;

namespace hcl_net.Parse.HCL
{
    internal class Token
    {
        private readonly TokenType _type;
        private readonly Pos _pos;
        private readonly string _text;
        private readonly bool _isJson;

        public Token(TokenType type, Pos pos, string text, bool isJson)
        {
            _type = type;
            _pos = pos;
            _text = text;
            _isJson = isJson;
        }

        public TokenType Type
        {
            get { return _type; }
        }

        public Pos Pos
        {
            get { return _pos; }
        }

        public string Text
        {
            get { return _text; }
        }

        public bool IsJson
        {
            get { return _isJson; }
        }

        public object Value
        {
            get { return GetValue(); }
        }

        /// <summary>
        /// Try to parse/extract the strongly typed
        /// value from the text. Will throw an exception
        /// if this fails.
        /// </summary>
        /// <returns></returns>
        private object GetValue()
        {
            try
            {
                switch (Type)
                {
                    case TokenType.BOOL:
                        return bool.Parse(Text);
                    case TokenType.FLOAT:
                        // Using double to support 64-bit float values
                        return double.Parse(Text);
                    case TokenType.NUMBER:
                        // Using long to support 64-bit int values
                        return long.Parse(Text);
                    case TokenType.IDENT:
                        return Text;
                    case TokenType.HEREDOC:
                        return UnindentHeredoc(Text);
                    case TokenType.STRING:
                        // The string might be quoted in which case we need to unquote it

                        // Simple case
                        if (string.IsNullOrWhiteSpace(Text))
                            return Text;
                        // The unquote method varies based on the format 
                        // of the text
                        return IsJson ? JsonUnquote(Text) : HclUnquote(Text);
                    default:
                        throw new ApplicationException("Cannot parse TokenType " + Type);

                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    string.Format("Failed to parse {0} as {1}", Text, Type), ex);
            }
        }

        /// <summary>
        /// A HEREDOC takes the form:
        /// <![CDATA[<<XXX
        /// Some text
        /// On multiple lines
        /// XXX
        /// ]]>
        /// And we want to strip the opening and closing tag lines.
        /// 
        /// In addition, if the first line is <![CDATA[<<-XXX]]> then
        /// we want to unindent the following lines (to make
        /// file formatting nicer).
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string UnindentHeredoc(string text)
        {
            var firstNewlineIdx = text.IndexOf("\n", StringComparison.InvariantCulture);
            if (firstNewlineIdx < 0)
            {
                throw new ApplicationException("Heredoc must contain a newline");
            }
            if (text.Last() != '\n')
            {
                throw new ApplicationException("Heredoc must end with a newline");
            }

            // Strip the opening line (+\n) and the ending HEREDOC delimiter:
            // "<<BLAH\n
            // Foo\n
            // Bar\n
            // BLAH\n"
            //   -->
            // "Foo\n
            // Bar\n
            // "
            var unindent = text[2] == '-';
            var delimeterLength = firstNewlineIdx 
                - 2 /*<<*/ 
                - (unindent ? 1 : 0) /* - if we unindent */;
            var strippedString = text.Substring(
                // Start from just after the first \n
                firstNewlineIdx + 1, 
                // And take away from the total length...
                text.Length - 
                // The first line...
                (firstNewlineIdx + 1) - 
                // The final delimeter
                delimeterLength -
                // And the final newline
                1);

            if (!unindent)
            {
                // If we don't need to unindent, just return the stripped string unaltered
                // Note we can assume that the last line is the same length as
                //  the first line
                return strippedString;
            }

            // Split the remaining string into lines
            var lines = strippedString
                .Split('\n');
            // The ammount of indentation is defined by the amount
            //  of whitespace at the front of the last line
            var whitespacePrefix = lines[lines.Length - 1];
            // If each line doesn't start with the whitespace prefix
            //  then it's not 'indented'...
            var isIndented = lines.All(l => l.StartsWith(whitespacePrefix));
            if (!isIndented)
            {
                // ... and we can return the heredoc as is,
                // with the leading space from the marker on
                // the final line stripped
                return strippedString.TrimEnd(' ', '\t');
            }
            // Otherwise, strip the prefix from each line
            // and rejoin them
            return string.Join("\n",
                lines.Take(lines.Length-1).Select(l => l.Substring(whitespacePrefix.Length))) + "\n";
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Pos, Type, Text);
        }

        private string JsonUnquote(string text)
        {
            throw new NotImplementedException();
        }

        private string HclUnquote(string text)
        {
            string error;
            var result = text.UnquoteHclString(out error);
            if (error != null)
                throw new ApplicationException("Error unquoting string: " + error);
            return result;
        }
    }
}