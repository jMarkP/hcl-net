using System.Collections.Generic;
using System.Linq;
using hcl_net.Parse.HCL.AST;

namespace hcl_net.Parse.HCL
{
    class Parser
    {
        private const string ErrEofToken = "EOF token found";
        private readonly Scanner _scanner;

        /// <summary>
        /// Last read token
        /// </summary>
        private Token _token;
        private Token _commaPrev;

        private CommentGroup[] _comments;
        /// <summary>
        /// Last lead comment
        /// </summary>
        private CommentGroup _leadComment;
        /// <summary>
        /// Last line comment
        /// </summary>
        private CommentGroup _lineComment;

        private int _indent;
        /// <summary>
        /// Buffer size (max = 1)
        /// </summary>
        private int _n;

        public Parser(string input)
            : this(input, null)
        {
        }

        public Parser(string input, string filename)
        {
            _scanner = new Scanner(input, filename);
        }

        public File Parse(out string error)
        {
            var scannerErr = "";
            string parseErr;
            _scanner.Error += (pos, message) =>
            {
                scannerErr = scannerErr + message + "\r\n";
            };

            ObjectList objectList = ParseObjectList(false, out parseErr);

            if (!string.IsNullOrEmpty(scannerErr))
            {
                error = scannerErr;
                return null;
            }
            if (!string.IsNullOrEmpty(parseErr))
            {
                error = parseErr;
                return null;
            }
            error = null;
            return new File(objectList, _comments);
        }

        /// <summary>
        /// Parse a list of items within an object (generally
        /// k/v/ pairs). inObj tells us whether we are within an
        /// object (braces: '{', '}') or just at the top level. If
        /// within an object we end at an RBRACE
        /// </summary>
        /// <param name="inObj"></param>
        /// <param name="parseErr"></param>
        /// <returns></returns>
        private ObjectList ParseObjectList(bool inObj, out string parseErr)
        {
            parseErr = null;
            var items = new List<ObjectItem>();

            while (true)
            {
                if (inObj)
                {
                    Scan();
                    Unscan();
                    if (_token.Type == TokenType.RBRACE)
                    {
                        break;
                    }
                }
                var item = ParseObjectItem(out parseErr);
                if (!string.IsNullOrEmpty(parseErr))
                {
                    // Not really an error in this case
                    if (parseErr == ErrEofToken)
                    {
                        parseErr = null;
                    }
                    break;
                }

                items.Add(item);

                // Object lists can be optionally comma-delimited, e.g.
                // when a list of maps is being expressed, so a comma is
                // allowed here
                Scan();
                if (_token.Type != TokenType.COMMA)
                {
                    Unscan();
                }

            }
            return new ObjectList(items);
        }

        internal ObjectItem ParseObjectItem(out string parseError)
        {
            var keys = ParseObjectKey(out parseError);
            if (keys.Length > 0 && parseError == ErrEofToken)
            {
                // We ignore eof token here since it is an error if we
                // didin't receive a value (but we did receive a key)
                // for the item
                parseError = null;
            }
            if (keys.Length > 0
                && parseError != null
                && _token.Type == TokenType.RBRACE)
            {
                // This is a strange boolean statement, but what it means is:
                // We have keys with no value, and we're likely in an object
                // (since RBrace ends an object). For this, we set err to nil so
                // we continue and get the error below of having the wrong value
                // type.
                parseError = null;

                // Reset the token type so we don't think it completed fine. See
                // objectType which uses p.tok.Type to check if we're done with
                // the object.
                _token = new Token(TokenType.EOF, _token.Pos, null, _token.IsJson);
            }
            if (parseError != null)
            {
                return null;
            }

            var leadComment = _leadComment;
            _leadComment = null;

            Pos assign;
            INode val;

            switch (_token.Type)
            {
                case TokenType.ASSIGN:
                    assign = _token.Pos;
                    val = ParseObject(out parseError);
                    if (parseError != null)
                    {
                        return null;
                    }
                    break;
                case TokenType.LBRACE:
                    assign = default(Pos);
                    val = ParseObjectType(out parseError);
                    if (parseError != null)
                    {
                        return null;
                    }
                    break;
                default:
                    parseError = string.Format("key '{0}' expected start of object ('{') or assignment ('=')",
                        string.Join(" ", keys.Select(k => k.Token.Text)));
                    return null;
            }

            // Do a look ahead for a line comment
            Scan();
            CommentGroup lineComment = null;
            if (keys.Length > 0
                && val.Pos.Line == keys[0].Pos.Line)
            {
                lineComment = _lineComment;
                _lineComment = null;
            }
            Unscan();

            return new ObjectItem(keys, assign, val, leadComment, lineComment);
        }

        private ObjectKey[] ParseObjectKey(out string parseError)
        {
            parseError = null;
            var keys = new List<ObjectKey>();
            while (true)
            {
                Scan();
                switch (_token.Type)
                {
                    case TokenType.EOF:
                        // It is very important to also return the keys here as well as
                        // the error. This is because we need to be able to tell if we
                        // did parse keys prior to finding the EOF, or if we just found
                        // a bare EOF.
                        parseError = ErrEofToken;
                        return keys.ToArray();
                    case TokenType.ASSIGN:
                        // assignment or oject only, but not nested objects. This
                        // is not allowed: "foo bar = {}"
                        if (keys.Count > 1)
                        {
                            parseError = string.Format("Nested object expected: LBRACE, got: {0}", _token.Type);
                            return null;
                        }
                        if (keys.Count == 0)
                        {
                            parseError = "No object keys found";
                            return null;
                        }
                        return keys.ToArray();
                    case TokenType.LBRACE:
                        // If we have no keys then it's a syntax error, e.g. {{}} is not
                        // allowed
                        if (keys.Count == 0)
                        {
                            parseError = string.Format("Expected IDENT | STRING, got: {0}", _token.Type);
                        }
                        return keys.ToArray();
                    case TokenType.IDENT:
                    case TokenType.STRING:
                        keys.Add(new ObjectKey(_token));
                        break;
                    default:
                        parseError = string.Format("expected: IDENT | STRING | ASSIGN | LBRACE, got: {0}", _token.Type);
                        return keys.ToArray();
                }
            }
        }

        private INode ParseObject(out string parseError)
        {
            Scan();
            switch (_token.Type)
            {
                case TokenType.NUMBER:
                case TokenType.FLOAT:
                case TokenType.BOOL:
                case TokenType.STRING:
                case TokenType.HEREDOC:
                    return ParseLiteralType(out parseError);
                case TokenType.LBRACE:
                    return ParseObjectType(out parseError);
                case TokenType.LBRACK:
                    return ParseListType(out parseError);
                case TokenType.COMMENT:
                    // Implement comment
                    break;
                case TokenType.EOF:
                    parseError = ErrEofToken;
                    return null;
            }
            parseError = string.Format("Unknown token: {0}", _token);
            return null;
        }

        private ObjectType ParseObjectType(out string parseError)
        {
            // Assume that the current token is an lbrace
            var lBracePos = _token.Pos;

            var list = ParseObjectList(true, out parseError);

            // if we hit RBRACE, we are good to go (means we parsed all Items), if it's
            // not a RBRACE, it's an syntax error and we just return it.
            if (!string.IsNullOrEmpty(parseError)
                && _token.Type != TokenType.RBRACE)
            {
                return null;
            }

            // No error, scan and expect the ending to be a brace
            Scan();
            if (_token.Type != TokenType.RBRACE)
            {
                parseError = string.Format("Object expected close RBRACE got: {0}", _token.Type);
                return null;
            }

            var rBracePos = _token.Pos;

            parseError = null;
            return new ObjectType(lBracePos, list, rBracePos);
        }

        private ListType ParseListType(out string parseError)
        {
            // We assume that the current scanned token is a LBRACK
            var lBrackPos = _token.Pos;
            var items = new List<INode>();

            var needComma = false;
            while (true)
            {
                Scan();
                if (needComma)
                {
                    if (_token.Type != TokenType.COMMA
                        && _token.Type != TokenType.RBRACK)
                    {
                        parseError = string.Format("Error parsing list. Expected comma or list end, got: {0}",
                            _token.Type);
                        return null;
                    }
                }
                switch (_token.Type)
                {
                    case TokenType.NUMBER:
                    case TokenType.FLOAT:
                    case TokenType.STRING:
                    case TokenType.HEREDOC:
                        var literal = ParseLiteralType(out parseError);
                        if (!string.IsNullOrEmpty(parseError))
                        {
                            return null;
                        }

                        // If there's a lead comment, apply it
                        if (_leadComment != null)
                        {
                            literal.LeadComment = _leadComment;
                            // And consume it
                            _leadComment = null;
                        }

                        items.Add(literal);
                        needComma = true;
                        break;
                    case TokenType.COMMA:
                        // Get next list item, or we are at the end
                        // Do a lookahead for possible line comment
                        Scan();
                        // Did we just read a comment?
                        if (_lineComment != null && items.Count > 0)
                        {
                            var lastLiteral = items.Last() as LiteralType;
                            if (lastLiteral != null)
                            {
                                lastLiteral.LineComment = _lineComment;
                                _lineComment = null;
                            }
                        }
                        Unscan();
                        needComma = false;
                        break;
                    case TokenType.LBRACE:
                        // Looks like a nested object, so parse it out
                        var objectType = ParseObjectType(out parseError);
                        if (!string.IsNullOrEmpty(parseError))
                        {
                            return null;
                        }
                        items.Add(objectType);
                        needComma = true;
                        break;
                    case TokenType.RBRACK:
                        // Finished;
                        var rBrackPos = _token.Pos;
                        parseError = null;
                        return new ListType(lBrackPos, items, rBrackPos);
                    case TokenType.BOOL:
                    case TokenType.LBRACK:
                    // Not supported by upstream implementation yet
                    default:
                        parseError = string.Format("Unexpected token while parsing list: {0}", _token.Type);
                        return null;
                }
            }
        }

        private LiteralType ParseLiteralType(out string parseError)
        {
            parseError = null;
            return new LiteralType(_token, null, null);
        }

        private CommentGroup ConsumeCommentGroup(int n, out int endLine)
        {
            var list = new List<Comment>();
            endLine = _token.Pos.Line;

            while (_token.Type == TokenType.COMMENT
                   && _token.Pos.Line <= endLine + n)
            {
                list.Add(ConsumeComment(out endLine));
            }
            return new CommentGroup(list);
        }

        private Comment ConsumeComment(out int endLine)
        {
            endLine = _token.Pos.Line;

            // Count the endLine if it's a multiline comment
            // (i.e. starting with /*)
            if (_token.Text.Length > 1
                && _token.Text[0] == '*')
            {
                endLine += _token.Text.Count(c => c == '\n');
            }
            var comment = new Comment(_token.Pos, _token.Text);
            _token = _scanner.Scan();
            return comment;
        }

        /// <summary>
        /// Scan returns the next token from the underlying scanner.
        /// If a token has been unscanned then read that instead.
        /// In the process if collects any comment groups encountered
        /// and remembers the last lead and line comments
        /// </summary>
        /// <returns></returns>
        private void Scan()
        {
            // If we have a token on the buffer, then return it
            if (_n != 0)
            {
                _n = 0;
                return;
            }

            // Otherwise consume the next token from the scanner
            // and save it to the buffer in case we
            // unscan later
            var prev = _token;
            _token = _scanner.Scan();

            if (_token.Type == TokenType.COMMENT)
            {
                CommentGroup comment = null;
                int endLine;

                if (_token.Pos.Line == prev.Pos.Line)
                {
                    // Comment is on the same line as the previous token;
                    // it cannot be a lead comment, but may be a line comment
                    comment = ConsumeCommentGroup(0, out endLine);
                    if (_token.Pos.Line != endLine)
                    {
                        // The next token is on a different line, thus
                        // the last comment group is a line comment
                        _lineComment = comment;
                    }
                }

                // Consume successor comments, if any
                endLine = -1;
                if (_token.Type == TokenType.COMMENT)
                {
                    comment = ConsumeCommentGroup(1, out endLine);
                }

                if (endLine + 1 == _token.Pos.Line && _token.Type != TokenType.RBRACE)
                {
                    if (_token.Type != TokenType.RBRACE
                        && _token.Type != TokenType.RBRACK)
                    {
                        // The next token is following on the line immediately
                        // after the comment group. Thus the last comment group
                        // is a lead comment.
                        _leadComment = comment;
                    }
                }
            }
        }

        private void Unscan()
        {
            _n = 1;
        }
    }

}
