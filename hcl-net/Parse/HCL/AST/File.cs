using System;

namespace hcl_net.Parse.HCL.AST
{
    /// <summary>
    /// File is a single HCL file
    /// </summary>
    class File : INode
    {
        private INode _node;
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

        public INode Walk(WalkFunc fn)
        {
            // Visit this node
            INode rewritten;
            if (!fn(this, out rewritten))
            {
                return rewritten;
            }
            var file = rewritten as File;
            if (file == null)
                throw new InvalidOperationException("Walk function returned wrong type");

            // Visit the child node of this node
            file._node = file.Node.Walk(fn);
            
            INode _;
            fn(null, out _);

            return file;
        }
    }
}