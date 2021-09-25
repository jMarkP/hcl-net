using System;
using System.Collections;
using System.Collections.Generic;

namespace hcl_net.v2
{
    internal class Traversal : IEnumerable<ITraverser>, IList<ITraverser>
    {
        public string RootName
        {
            get
            {
                if (IsRelative)
                {
                    throw new Exception("Can't use RootName on a relative traversal");
                }

                return ((TraverseRoot) this[0]).Name;
            }
        }

        public bool IsRelative
        {
            get
            {
                if (Count == 0)
                {
                    return true;
                }

                if (this[0] is TraverseRoot)
                {
                    return false;
                }

                return true;
            }
        }
        
        #region IList implementation
        private readonly List<ITraverser> _items = new List<ITraverser>();
        public IEnumerator<ITraverser> GetEnumerator()
        {
            return ((IEnumerable<ITraverser>)_items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _items).GetEnumerator();
        }

        public void Add(ITraverser item)
        {
            _items.Add(item);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(ITraverser item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(ITraverser[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public bool Remove(ITraverser item)
        {
            return _items.Remove(item);
        }

        public int Count => _items.Count;

        public bool IsReadOnly => ((ICollection<ITraverser>) _items).IsReadOnly;

        public int IndexOf(ITraverser item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, ITraverser item)
        {
            _items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        public ITraverser this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }
        #endregion
    }

    internal class TraverseRoot : ITraverser
    {
        public TraverseRoot(string name, Range srcRange)
        {
            Name = name;
            SrcRange = srcRange;
        }

        public string Name { get; }
        public Range SrcRange { get; }
    }
}