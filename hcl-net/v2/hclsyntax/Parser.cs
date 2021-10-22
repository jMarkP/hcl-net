using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace hcl_net.v2.hclsyntax
{
    internal static class AsHclExtensions
    {
        public static hcl_net.v2.Block? AsHclBlock(this Block? b)
        {
            if (b == null) return null;
            return new v2.Block(
                b.Type,
                b.Labels,
                b.Body,
                b.DefRange,
                b.TypeRange,
                b.LabelRanges);
        }

        public static hcl_net.v2.Attribute? AsHCLAttribute(this Attribute? a)
        {
            if (a == null) return null;
            return new v2.Attribute(
                a.Name,
                a.Expression,
                a.SrcRange,
                a.NameRange);
        }
    }
    
    internal class Attribute : INode
    {
        public Attribute(string name, IExpression expression, Range srcRange, Range nameRange, Range equalsRange)
        {
            Name = name;
            Expression = expression;
            SrcRange = srcRange;
            NameRange = nameRange;
            EqualsRange = equalsRange;
        }

        public string Name { get; }
        public IExpression Expression { get; }
        public Range SrcRange { get; }
        public Range NameRange { get; }
        public Range EqualsRange { get; }
        public void WalkChildNodes(Action<INode> walker)
        {
            walker(Expression);
        }

        public Range Range => SrcRange;

    }

    internal class Attributes : IDictionary<string, Attribute>, INode
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

        public Range Range
        {
            get
            {
                // An attributes doesn't really have a useful range to report, since
                // it's just a grouping construct. So we'll arbitrarily take the
                // range of one of the attributes, or produce an invalid range if we have
                // none. In practice, there's little reason to ask for the range of
                // an Attributes.
                return _items.Any()
                    ? _items.First().Value.Range
                    : new Range("<unknown", default, default);
            }
        }

        public void WalkChildNodes(Action<INode> walk)
        {
            foreach (var attr in Values)
            {
                walk(attr);
            }
        }
    }
    
    
    internal class Blocks : IEnumerable<Block>, INode
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

        public Range Range
        {
            get
            {
                return _items.Any()
                    ? _items[0].Range
                    : new Range("<unknown>", default, default);
            }
        }

        public void WalkChildNodes(Action<INode> walker)
        {
            foreach (var block in _items)
            {
                walker(block);
            }
        }
    }

    internal class Block : INode
    {
        public Block(string type, string[] labels, Body body, Range typeRange, Range[] labelRanges, Range openBraceRange, Range closeBraceRange)
        {
            Type = type;
            Labels = labels;
            Body = body;
            TypeRange = typeRange;
            LabelRanges = labelRanges;
            OpenBraceRange = openBraceRange;
            CloseBraceRange = closeBraceRange;
        }

        public string Type { get; }
        public string[] Labels { get; }
        public Body Body { get; }
        
        public Range TypeRange { get; }
        public Range[] LabelRanges { get; }
        public Range OpenBraceRange { get; }
        public Range CloseBraceRange { get; }
        
        public Range DefRange
        {
            get
            {
                var lastHeaderRange = TypeRange;
                if (LabelRanges.Any())
                {
                    lastHeaderRange = LabelRanges.Last();
                }

                return Range.Between(TypeRange, lastHeaderRange);
            }
        }

        public Range Range => Range.Between(TypeRange, CloseBraceRange);
        public void WalkChildNodes(Action<INode> func)
        {
            func(Body);
        }
    }
    internal class Body : IBody, INode
    {
        public Body(Attributes attributes, Blocks blocks, Dictionary<string, object> hiddenAttrs, Dictionary<string, object> hiddenBlocks, Range srcRange, Range endRange)
        {
            Attributes = attributes;
            Blocks = blocks;
            HiddenAttrs = hiddenAttrs;
            HiddenBlocks = hiddenBlocks;
            SrcRange = srcRange;
            EndRange = endRange;
        }

        public Attributes Attributes { get; }
        public Blocks Blocks { get; }

        private Dictionary<string, object> HiddenAttrs { get; }
        private Dictionary<string, object> HiddenBlocks { get; }
        
        public Range SrcRange { get; }
        public Range EndRange { get; }
        
        public (BodyContent, Diagnostics) Content(BodySchema schema)
        {
            var (content, remainHCL, diags) = PartialContent(schema);

            // Now we'll see if anything actually remains, to produce errors about
            // extraneous items.
            var remain = (Body) remainHCL;

            foreach (var kvp in Attributes)
            {
                var name = kvp.Key;
                var attr = kvp.Value;
                if (!remain.HiddenAttrs.ContainsKey(name))
                {
                    var suggestions = schema.Attributes
                        .Where(attrS => !content.Attributes.ContainsKey(attrS.Name))
                        .Select(attrS => attrS.Name)
                        .ToArray();
                    var suggestion = DidYouMeanExtensions.NameSuggestion(name, suggestions);
                    if (!string.IsNullOrEmpty(suggestion))
                    {
                        suggestion = $" Did you mean {suggestion}?";
                    }
                    else
                    {
                        // Is there a block of the same name?
                        var matchingBlockType = schema.Blocks.FirstOrDefault(b => b.Type == name);
                        suggestion = matchingBlockType != null
                            ? $" Did you mean to define a block of type {matchingBlockType.Type}"
                            : "";
                    }

                    diags = diags.Append(new Diagnostic(
                        severity: DiagnosticSeverity.Error,
                        summary: "Unsupported argument",
                        detail: $"An argument named {name} is not expected here.{suggestion}",
                        subject: attr.NameRange));
                }
            }

            foreach (var block in Blocks)
            {
                var blockType = block.Type;
                if (!remain.HiddenBlocks.ContainsKey(blockType))
                {
                    var suggestions = schema.Blocks
                        .Select(blockS => blockS.Type)
                        .ToArray();
                    var suggestion = DidYouMeanExtensions.NameSuggestion(blockType, suggestions);
                    if (!string.IsNullOrEmpty(suggestion))
                    {
                        suggestion = $" Did you mean {suggestion}?";
                    }
                    else
                    {
                        // Is there an attribute of the same name?
                        var matchingAttribute = schema.Attributes.FirstOrDefault(b => b.Name == blockType);
                        suggestion = matchingAttribute != null
                            ? $" Did you mean to define argument {blockType}? If so, use the equals sign to assign it a value"
                            : "";
                    }
                    
                    diags = diags.Append(new Diagnostic(
                        severity: DiagnosticSeverity.Error,
                        summary: "Unsupported block type",
                        detail: $"Blocks of type {blockType} are not expected here.{suggestion}",
                        subject: block.TypeRange));
                }
            }

            return (content, diags);
        }

        public (BodyContent, IBody, Diagnostics) PartialContent(BodySchema schema)
        {
            var attrs = new v2.Attributes();
            var blocks = new v2.Blocks(new v2.Block[0]);
            var hiddenAttrs = new Dictionary<string, object>(HiddenAttrs ?? new());
            var hiddenBlocks = new Dictionary<string, object>(HiddenBlocks ?? new());

            var diags = Diagnostics.None;
            
            foreach (var attribute in schema.Attributes)
            {
                var name = attribute.Name;
                var exists = Attributes.TryGetValue(name, out var attr);
                var hidden = hiddenAttrs.ContainsKey(name);
                if (hidden || !exists)
                {
                    if (attribute.Required)
                    {
                        diags = diags.Append(new Diagnostic(
                            severity: DiagnosticSeverity.Error,
                            summary: "Missing required argument",
                            detail: $"The argument {attribute.Name} is required, but no definition was found.",
                            subject: MissingItemRange()));
                    }
                    continue;
                }

                hiddenAttrs[name] = new object();
                attrs[name] = attr.AsHCLAttribute()!;
            }

            var blocksWanted = schema.Blocks.ToDictionary(b => b.Type);
            foreach (var block in Blocks)
            {
                if (hiddenBlocks.ContainsKey(block.Type))
                {
                    continue;
                }

                if (!blocksWanted.TryGetValue(block.Type, out var blockS))
                {
                    continue;
                }

                if (block.Labels.Length > blockS.LabelNames.Length)
                {
                    var name = block.Type;
                    if (blockS.LabelNames.Length == 0)
                    {
                        diags = diags.Append(new Diagnostic(
                            severity: DiagnosticSeverity.Error,
                            summary: $"Extraneous label for {name}",
                            detail: $"No labels are expected for {name} blocks.",
                            subject: block.LabelRanges[0],
                            context: Range.Between(block.TypeRange, block.OpenBraceRange)));
                    }
                    else
                    {
                        diags = diags.Append(new Diagnostic(
                            severity: DiagnosticSeverity.Error,
                            summary: $"Extraneous label for {name}",
                            detail: $"Only {blockS.LabelNames.Length} labels ({string.Join(", ", blockS.LabelNames)}) are expected for {name} blocks.",
                            subject: block.LabelRanges[blockS.LabelNames.Length],
                            context: Range.Between(block.TypeRange, block.OpenBraceRange)));
                    }
                    continue;
                }

                if (block.Labels.Length < blockS.LabelNames.Length)
                {
                    var name = block.Type;
                    diags = diags.Append(new Diagnostic(
                        severity: DiagnosticSeverity.Error,
                        summary: $"Missing {blockS.LabelNames[block.Labels.Length]} for {name}",
                        detail: $"All {name} blocks must have {blockS.LabelNames.Length} labels ({string.Join(", ", blockS.LabelNames)}).",
                        subject: block.OpenBraceRange,
                        context: Range.Between(block.TypeRange, block.OpenBraceRange)));
                    continue;
                }

                blocks = blocks.Append(block.AsHclBlock()!);
            }
            // We hide blocks only after we've processed all of them, since otherwise
            // we can't process more than one of the same type.
            foreach (var blockS in schema.Blocks)
            {
                hiddenBlocks[blockS.Type] = new object();
            }

            var remain = new Body(
                Attributes,
                Blocks,
                hiddenAttrs,
                hiddenBlocks,
                SrcRange,
                EndRange
            );
            return (
                new BodyContent(attrs, blocks, MissingItemRange()),
                remain,
                diags);
        }

        public (v2.Attributes, Diagnostics) JustAttributes()
        {
            var attrs = new v2.Attributes();
            var diags = Diagnostics.None;

            if (Blocks.Any())
            {
                var example = Blocks.First();
                diags = diags.Append(new Diagnostic(
                    severity: DiagnosticSeverity.Error,
                    summary: $"Unexpected {example.Type} block",
                    detail: "Blocks are not allowed here",
                    subject: example.TypeRange));
                // we will continue processing anyway, and return the attributes
                // we are able to find so that certain analyses can still be done
                // in the face of errors.
            }

            if (!Attributes.Any())
            {
                return (attrs, diags);
            }

            foreach (var kvp in Attributes)
            {
                var name = kvp.Key;
                var attr = kvp.Value;
                if (HiddenAttrs.ContainsKey(name))
                {
                    continue;
                }

                attrs[name] = attr.AsHCLAttribute()!;
            }

            return (attrs, diags);
        }

        public Range MissingItemRange()
        {
            return new Range(SrcRange.Filename, SrcRange.Start, SrcRange.End);
        }

        public Range Range => SrcRange;
        public void WalkChildNodes(Action<INode> func)
        {
            func(Attributes);
            func(Blocks);
        }
    }
    internal class Parser
    {
        public (Body, Diagnostics) ParseBody(TokenType end)
        {
            
        }
    }
}