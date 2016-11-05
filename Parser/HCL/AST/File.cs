namespace hcl_net.Parser.HCL.AST
{
    /// <summary>
    /// File is a single HCL file
    /// </summary>
    class File : INode
    {
        private readonly INode _node;
        private readonly CommentGroup[] _comments;

        public File(INode node, CommentGroup[] comments)
        {
            _node = node;
            _comments = comments;
        }

        /// <summary>
        /// Root node - normally an ObjectList
        /// </summary>
        public INode Node
        {
            get { return _node; }
        }

        /// <summary>
        /// List of all comments in the source
        /// </summary>
        public CommentGroup[] Comments
        {
            get { return _comments; }
        }

        public Pos Pos
        {
            get { return Node.Pos; }
        }
    }
}