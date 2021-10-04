using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using cty_net;

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
                    throw new Exception($"Can't use {nameof(RootName)} on a relative traversal");
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
        
        public (Value, Diagnostics) TraverseRel(Value val)
        {
            if (!IsRelative)
            {
                throw new Exception($"Can't use {nameof(TraverseRel)} on an absolute traversal");
            }

            var current = val;
            var diags = new Diagnostics();
            foreach (var tr in this)
            {
                Diagnostics newDiags;
                (current, newDiags) = tr.TraversalStep(current);
                diags = diags.Append(newDiags);
                if (newDiags.HasErrors)
                {
                    return (Value.DynamicVal, diags);
                }
            }
            
            return (current, diags);
        }

        public (Value, Diagnostics) TraverseAbs(EvalContext ctx)
        {
            if (IsRelative)
            {
                throw new Exception($"Can't use {nameof(TraverseAbs)} on a relative traversal");
            }

            var split = SimpleSplit();
            var root = split.Abs[0] as TraverseRoot;
            var name = root.Name;
            var hasVariables = false;
            EvalContext? currCtx = ctx;
            while (currCtx != null)
            {
                if (!currCtx.Variables.Any())
                {
                    currCtx = currCtx.Parent;
                    continue;
                }

                hasVariables = true;
                if (currCtx.Variables.TryGetValue(name, out var val))
                {
                    return split.Rel.TraverseRel(val);
                }

                currCtx = currCtx.Parent;
            }
            if (hasVariables)
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
            SourceRange = srcRange;
        }

        public string Name { get; }
        public Range SourceRange { get; }
        
        public (Value, Diagnostics) TraversalStep(Value val)
        {
            throw new NotImplementedException("Cannot traverse an absolute traversal");
        }
    }
}