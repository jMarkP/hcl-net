using System;
using hcl_net.Utilities;

namespace hcl_net.Parser.HCL
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
                        // Determine the Unquote method to use. If it came from JSON,
                        // then we need to use the built-in unquote since we have to
                        // escape interpolations there.

                        // Simple case
                        if (string.IsNullOrWhiteSpace(Text))
                            return Text;
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

        private object UnindentHeredoc(string text)
        {
            throw new NotImplementedException();
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
            var result = HclQuoteUtil.Unquote(text, out error);
            if (error != null)
                throw new ApplicationException("Error unquoting string: " + error);
            return result;
        }
    }
}