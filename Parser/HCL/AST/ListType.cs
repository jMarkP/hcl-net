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
    }
}