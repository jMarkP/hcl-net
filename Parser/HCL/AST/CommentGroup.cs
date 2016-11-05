using System;
using System.Collections.Generic;
using System.Linq;

namespace hcl_net.Parser.HCL.AST
{
    /// <summary>
    /// A sequence of comments with no other
    /// tokens and no empty lines between
    /// </summary>
    class CommentGroup : INode
    {
        private readonly Comment[] _list;

        public CommentGroup(IEnumerable<Comment> list)
        {
            if (!list.Any())
                throw new ArgumentException("Must have at least item");
            _list = list.ToArray();
        }

        /// <summary>
        /// Constitiuent comments
        /// </summary>
        public Comment[] List
        {
            get { return _list; }
        }

        public Pos Pos
        {
            get { return List[0].Pos; }
        }
    }
}