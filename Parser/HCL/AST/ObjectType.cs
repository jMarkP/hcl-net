using System.Collections.Generic;
using System.Linq;

namespace hcl_net.Parser.HCL.AST
{
    /// <summary>
    /// An HCL Object Type
    /// </summary>
    class ObjectType : INode
    {
        private readonly Pos _lbrace;
        private readonly Pos _rbrace;
        private readonly ObjectList[] _list;

        public ObjectType(Pos lbrace, IEnumerable<ObjectList> list, Pos rbrace)
        {
            _lbrace = lbrace;
            _list = list.ToArray();
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
        public INode[] List
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
    }
}