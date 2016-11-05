using System.Collections.Generic;
using System.Linq;

namespace hcl_net.Parser.HCL.AST
{
    /// <summary>
    /// An HCL Object Item. An item is represented with a key (or keys).
    /// It can be an assignment, or an object (both normal and nested)
    /// </summary>
    internal class ObjectItem : INode
    {
        private readonly ObjectKey[] _keys;
        private readonly Pos _assign;
        private readonly INode _val;
        private readonly CommentGroup _leadComment;
        private readonly CommentGroup _lineComment;

        public ObjectItem(IEnumerable<ObjectKey> keys, Pos assign, INode val, CommentGroup leadComment, CommentGroup lineComment)
        {
            _keys = keys.ToArray();
            _assign = assign;
            _val = val;
            _leadComment = leadComment;
            _lineComment = lineComment;
        }

        /// <summary>
        /// If this is an assignment, then Keys will be
        /// length one. If it's a nested object it
        /// can be larger. In that case 'assign' is
        /// invalid
        /// </summary>
        public ObjectKey[] Keys
        {
            get { return _keys; }
        }

        /// <summary>
        /// The position of the '=' character (if any)
        /// </summary>
        public Pos Assign
        {
            get { return _assign; }
        }

        /// <summary>
        /// Val is the item itself. It can be an object, list,
        /// number, bool or a string. If the key length is larger than
        /// one, val can be only of type Objecct
        /// </summary>
        public INode Val
        {
            get { return _val; }
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
            get
            {
                return Keys.Any()
                    ? Keys.First().Pos
                    : default(Pos);
            }
        }

        public ObjectItem WithoutKeyPrefix(int prefixLength)
        {
            return new ObjectItem(Keys.Skip(prefixLength),
                Assign, Val, LeadComment, LineComment);
        }
    }
}