using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace hcl_net.v2
{
    /// <summary>
    /// Block represents a nested block within a Body.
    /// </summary>
    internal class Block
    {
        public Block(string type, string[] labels, IBody body, Range defRange, Range typeRange, Range[] labelRanges)
        {
            Type = type;
            Labels = labels;
            Body = body;
            DefRange = defRange;
            TypeRange = typeRange;
            LabelRanges = labelRanges;
        }

        public string Type { get; }
        public string[] Labels { get; }
        public IBody Body { get; }
        
        /// <summary>
        /// Range that can be considered the "definition" for seeking in an editor
        /// </summary>
        public Range DefRange { get; }
        /// <summary>
        /// Range for the block type declaration specifically.
        /// </summary>
        public Range TypeRange { get; }
        /// <summary>
        /// Ranges for the label values specifically.
        /// </summary>
        public Range[] LabelRanges { get; }
    }
    
    internal class Blocks : IEnumerable<Block>
    {
        private readonly Block[] _items;

        public Blocks(Block[] items)
        {
            _items = items;
        }

        public IEnumerator<Block> GetEnumerator()
        {
            return ((IEnumerable<Block>)_items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
        public int Length => _items.Length;
        
        public Blocks Append(Block item)
        {
            return new Blocks(((IEnumerable<Block>) this).Append(item).ToArray());
        }
        public Blocks Append(Blocks other)
        {
            // Avoid needless allocations if
            // append would be a no-op
            // (This class is immutable so this is safe)
            if (other.Length == 0)
            {
                return this;
            }
            if (this.Length == 0)
            {
                return other;
            }

            return new(this.Concat(other).ToArray());
        }
    }
}