using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace hcl_net.v2
{
    internal class Attribute
    {
        public Attribute(string name, IExpression expression, Range range, Range nameRange)
        {
            Name = name;
            Expression = expression;
            Range = range;
            NameRange = nameRange;
        }

        public string Name { get; }
        public IExpression Expression { get; }
        public Range Range { get; }
        public Range NameRange { get; }
    }
    
    internal class Attributes : IDictionary<string, Attribute>
    {
        private IDictionary<string, Attribute> _items = new Dictionary<string, Attribute>();
        public IEnumerator<KeyValuePair<string, Attribute>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _items).GetEnumerator();
        }

        public void Add(KeyValuePair<string, Attribute> item)
        {
            _items.Add(item);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(KeyValuePair<string, Attribute> item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, Attribute>[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, Attribute> item)
        {
            return _items.Remove(item);
        }

        public int Count => _items.Count;

        public bool IsReadOnly => _items.IsReadOnly;

        public void Add(string key, Attribute value)
        {
            _items.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _items.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return _items.Remove(key);
        }

        public bool TryGetValue(string key, out Attribute value)
        {
            return _items.TryGetValue(key, out value);
        }

        public Attribute this[string key]
        {
            get => _items[key];
            set => _items[key] = value;
        }

        public ICollection<string> Keys => _items.Keys;

        public ICollection<Attribute> Values => _items.Values;
    }
}