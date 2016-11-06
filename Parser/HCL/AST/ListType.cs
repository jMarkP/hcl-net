using System;
using System.Collections.Generic;
using System.Linq;

namespace hcl_net.Parser.HCL.AST
{
    /// <summary>
    /// A list of HCL nodes
    /// </summary>
    class ListType : INode
    {
        private readonly Pos _lbrack;
        private readonly Pos _rbrack;
        private readonly INode[] _list;

        public ListType(Pos lbrack, IEnumerable<INode> list, Pos rbrack)
        {
            _lbrack = lbrack;
            _list = list.ToArray();
            _rbrack = rbrack;
        }

        /// <summary>
        /// Position of the '['
        /// </summary>
        public Pos Lbrack
        {
            get { return _lbrack; }
        }

        /// <summary>
        /// Position of the ']'
        /// </summary>
        public Pos Rbrack
        {
            get { return _rbrack; }
        }

        /// <summary>
        /// The elements in lexical order
        /// </summary>
        public INode[] List
        {
            get { return _list; }
        }

        public Pos Pos
        {
            get
            {
                return Lbrack;
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
            var listType = rewritten as ListType;
            if (listType == null)
                throw new InvalidOperationException("Walk function returned wrong type");

            // Visit the child nodes of this node
            for (int i = 0; i < listType.List.Length; i++)
            {
                listType.List[i] = listType.List[i].Walk(fn);
            }

            INode _;
            fn(null, out _);

            return listType;
        }
    }
}