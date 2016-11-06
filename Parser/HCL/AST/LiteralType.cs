using System.Text;
using System.Threading.Tasks;

namespace hcl_net.Parser.HCL.AST
{
    class LiteralType : INode
    {
        private readonly Token _token;
        private readonly CommentGroup _leadComment;
        private readonly CommentGroup _lineComment;

        public LiteralType(Token token, CommentGroup leadComment, CommentGroup lineComment)
        {
            _token = token;
            _leadComment = leadComment;
            _lineComment = lineComment;
        }

        /// <summary>
        /// The token representing this key
        /// </summary>
        public Token Token
        {
            get { return _token; }
        }

        /// <summary>
        /// Associated lead comment
        /// </summary>
        public CommentGroup LeadComment
        {
            get { return _leadComment; }
        }

        /// <summary>
        /// Associated line comment
        /// </summary>
        public CommentGroup LineComment
        {
            get { return _lineComment; }
        }

        public Pos Pos
        {
            get { return Token.Pos; }
        }

        public INode Walk(WalkFunc fn)
        {
            // Visit this node
            INode rewritten;
            if (!fn(this, out rewritten))
            {
                return rewritten;
            }
            INode _;
            fn(null, out _);
            return rewritten;
        }
    }
}
