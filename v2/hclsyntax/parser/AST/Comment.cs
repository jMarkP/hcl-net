namespace hcl_net.v2.hclsyntax.parser.AST
{
    class Comment : INode
    {
        private readonly Pos _start;
        private readonly string _text;

        public Comment(Pos start, string text)
        {
            _start = start;
            _text = text;
        }

        /// <summary>
        /// Position of '/' or '#'
        /// </summary>
        public Pos Start
        {
            get { return _start; }
        }

        /// <summary>
        /// Contents of the comment
        /// </summary>
        public string Text
        {
            get { return _text; }
        }

        public Pos Pos
        {
            get { return Start; }
        }

        public INode Walk(WalkFunc fn)
        {
            throw new System.NotSupportedException();
        }
    }
}