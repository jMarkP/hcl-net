namespace hcl_net.Parser.HCL.AST
{
    /// <summary>
    /// Object keys are either an identifier or of type string
    /// </summary>
    class ObjectKey : INode
    {
        private readonly Token _token;

        public ObjectKey(Token token)
        {
            _token = token;
        }

        /// <summary>
        /// The token representing this key
        /// </summary>
        public Token Token
        {
            get { return _token; }
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