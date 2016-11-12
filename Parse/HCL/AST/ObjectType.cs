using System;

namespace hcl_net.Parse.HCL.AST
{
    /// <summary>
    /// An HCL Object Type
    /// </summary>
    class ObjectType : INode
    {
        private readonly Pos _lbrace;
        private readonly Pos _rbrace;
        private ObjectList _list;

        public ObjectType(Pos lbrace, ObjectList list, Pos rbrace)
        {
            _lbrace = lbrace;
            _list = list;
            _rbrace = rbrace;
        }

        /// <summary>
        /// Position of the '{'
        /// </summary>
        public Pos Lbrace
        {
            get { return _lbrace; }
        }

        /// <summary>
        /// Position of the '}'
        /// </summary>
        public Pos Rbrace
        {
            get { return _rbrace; }
        }

        /// <summary>
        /// The elements in lexical order
        /// </summary>
        public ObjectList List
        {
            get { return _list; }
        }

        public Pos Pos
        {
            get
            {
                return Lbrace;
            }
        }

        public INode Walk(WalkFunc fn)
        {
            // Visit this node
            INode rewritten;
            if (!fn(this, out rewritten))
            {
                return rewritten;
            }
            var objectType = rewritten as ObjectType;
            if (objectType == null)
                throw new InvalidOperationException("Walk function returned wrong type");

            // Visit the child node of this node
            objectType._list = (ObjectList)objectType._list.Walk(fn);

            INode _;
            fn(null, out _);

            return objectType;
        }
    }
}