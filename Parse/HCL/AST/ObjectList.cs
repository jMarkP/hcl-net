using System;
using System.Collections.Generic;
using System.Linq;

namespace hcl_net.Parse.HCL.AST
{
    /// <summary>
    /// A list of ObjetItems
    /// </summary>
    class ObjectList : INode
    {
        private readonly ObjectItem[] _items;
        public ObjectList(IEnumerable<ObjectItem> items)
        {
            _items = items.ToArray();
        }

        /// <summary>
        /// The items in the list
        /// </summary>
        public ObjectItem[] Items {
            get { return _items; } }
        
        /// <summary>
        /// Filter filters out the objects with the given key list as a prefix.
        ///
        /// The returned list of objects contain ObjectItems where the keys have
        /// this prefix already stripped off. This might result in objects with
        /// zero-length key lists if they have no children.
        ///
        /// If no matches are found, an empty ObjectList (non-nil) is returned.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public ObjectList Filter(params string[] keys)
        {
            var result = new List<ObjectItem>();
            foreach (var item in Items)
            {
                // Make sure there are enough keys to satisfy the query
                if (item.Keys.Length < keys.Length)
                    continue;
                var match = true;
                for (var i = 0; i < keys.Length; i++)
                {
                    var key = item.Keys[i].Token.Value as string;
                    if (!string.Equals(keys[i], key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        match = false;
                        break;
                    }
                }

                if (!match)
                    continue;

                // Strip off the prefix from the children
                ObjectItem newItem = item.WithoutKeyPrefix(keys.Length);
                result.Add(newItem);
            }

            return new ObjectList(result);
        }

        /// <summary>
        /// Children returns further nested objects (key length &gt; 0) within this
        /// ObjectList. This should be used with Filter to get at child items.
        /// </summary>
        /// <returns></returns>
        public ObjectList Children()
        {
            return new ObjectList(Items.Where(item => item.Keys.Length > 0));
        }

        /// <summary>
        /// Elem returns items in the list that are direct element assignments
        /// (key length == 0). This should be used with Filter to get at elements.
        /// </summary>
        /// <returns></returns>
        public ObjectList Elem()
        {
            return new ObjectList(Items.Where(item => item.Keys.Length == 0));
        }

        public Pos Pos
        {
            get
            {
                return _items.Any()
                    ? _items[0].Pos
                    : default(Pos);
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
            var objectList = rewritten as ObjectList;
            if (objectList == null)
                throw new InvalidOperationException("Walk function returned wrong type");

            // Visit the child nodes of this node
            for (int i = 0; i < objectList.Items.Length; i++)
            {
                objectList.Items[i] = (ObjectItem)objectList.Items[i].Walk(fn);
            }

            INode _;
            fn(null, out _);

            return objectList;
        }
    }
}