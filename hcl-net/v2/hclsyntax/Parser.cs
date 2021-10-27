using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public Blocks()
        {
            _items = Array.Empty<Block>();
        }
        
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
        public Body(Range srcRange, Range endRange)
        {
            Attributes = new Attributes();
            Blocks = new Blocks();
            HiddenAttrs = new Dictionary<string, object>();
            HiddenBlocks = new Dictionary<string, object>();
            SrcRange = srcRange;
            EndRange = endRange;
        }
        
        public Body(Attributes attributes, Blocks blocks, Dictionary<string, object> hiddenAttrs, Dictionary<string, object> hiddenBlocks, Range srcRange, Range endRange)
        {
            Attributes = attributes;
            Blocks = blocks;
            HiddenAttrs = hiddenAttrs;
            HiddenBlocks = hiddenBlocks;
            SrcRange = srcRange;
            EndRange = endRange;
        }
        
        public Body(Attributes attributes, Blocks blocks, Range srcRange, Range endRange)
        {
            Attributes = attributes;
            Blocks = blocks;
            HiddenAttrs = new Dictionary<string, object>();
            HiddenBlocks = new Dictionary<string, object>();
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
        private Range PrevRange { get; set; }
        private Range NextRange { get; set; }
        
        /// <summary>
        /// set to true if any recovery is attempted. The parser can use this
        /// to attempt to reduce error noise by suppressing "bad token" errors
        /// in recovery mode, assuming that the recovery heuristics have failed
        /// in this case and left the peeker in a wrong place.
        /// </summary>
        private bool Recovery { get; set; }
        
        public (Body, Diagnostics) ParseBody(TokenType end)
        {
            var attrs = new Attributes();
            var blocks = new Blocks();
            var diags = Diagnostics.None;

            var startRange = PrevRange;
            Range endRange;

            while (true)
            {
                var next = Peek();
                if (next.Type == end)
                {
                    endRange = NextRange;
                    Read();
                    goto ParseBodyLoopEnd;
                }

                switch (next.Type)
                {
                    case TokenType.TokenNewline:
                        Read();
                        continue;
                    case TokenType.TokenIdent:
                        var (item, itemDiags) = ParseBodyItem();
                        diags = diags.Append(itemDiags);
                        switch (item)
                        {
                            case Block block:
                                blocks = blocks.Append(block);
                                break;
                            case Attribute attr:
                                if (attrs.TryGetValue(attr.Name, out var existing))
                                {
                                    diags = diags.Append(new Diagnostic(
                                        severity: DiagnosticSeverity.Error,
                                        summary: "Attribute redefined",
                                        detail: $"The argument {attr.Name} was already set at {existing.NameRange.ToString()}. Each argument may be set only once.",
                                        subject: attr.NameRange));
                                }
                                else
                                {
                                    attrs[attr.Name] = attr;
                                }

                                break;
                            default:
                                // This should never happen for valid input, but may if a
                                // syntax error was detected in ParseBodyItem that prevented
                                // it from even producing a partially-broken item. In that
                                // case, it would've left at least one error in the diagnostics
                                // slice we already dealt with above.
                                //
                                // We'll assume ParseBodyItem attempted recovery to leave
                                // us in a reasonable position to try parsing the next item.
                                continue;
                        }
                        break;
                    default:
                        var bad = Read();
                        if (!Recovery)
                        {
                            if (bad.Type == TokenType.TokenOQuote)
                            {
                                diags = diags.Append(new Diagnostic(
                                    severity: DiagnosticSeverity.Error,
                                    summary: "Invalid argument name",
                                    detail: "Argument names must not be quoted",
                                    subject: bad.Range));
                            }
                            else
                            {
                                diags = diags.Append(new Diagnostic(
                                    severity: DiagnosticSeverity.Error,
                                    summary: "Argument or block definition required",
                                    detail: "An argument or block definition is required here.",
                                    subject: bad.Range));
                            }
                        }

                        endRange = PrevRange; // arbitrary, but somewhere inside the body means better diagnostics
                        Recover(end); // attempt to recover to the token after the end of this body
                        goto ParseBodyLoopEnd;
                }
            }
            ParseBodyLoopEnd:
            return (new Body(attrs, blocks, Range.Between(startRange, endRange),
                new Range(endRange.Filename, endRange.End, endRange.End)), diags);
        }
        
        public (INode?, Diagnostics) ParseBodyItem()
        {
            var ident = Read();
            if (ident.Type != TokenType.TokenIdent)
            {
                RecoverAfterBodyItem();
                return (null, new Diagnostics(new Diagnostic(
                    severity: DiagnosticSeverity.Error,
                    summary: "Argument or block definition required",
                    detail: "An argument or block definition is required here.",
                    subject: ident.Range)));
            }

            var next = Peek();
            switch (next.Type)
            {
                case TokenType.TokenEqual:
                    return FinishParsingBodyAttribute(ident, false);
                case TokenType.TokenOQuote:
                case TokenType.TokenOBrace:
                case TokenType.TokenIdent:
                    return FinishParsingBodyBlock(ident);
                default:
                    RecoverAfterBodyItem();
                    
                    return (null, new Diagnostics(new Diagnostic(
                        severity: DiagnosticSeverity.Error,
                        summary: "Argument or block definition required",
                        detail: "An argument or block definition is required here.",
                        subject: ident.Range)));
            }
        }

        /// <summary>
        /// parseSingleAttrBody is a weird variant of ParseBody that deals with the
        /// body of a nested block containing only one attribute value all on a single
        /// line, like foo { bar = baz } . It expects to find a single attribute item
        /// immediately followed by the end token type with no intervening newlines.
        /// </summary>
        /// <param name="end"></param>
        /// <returns></returns>
        private (Body?, Diagnostics) ParseSingleAttrBody(TokenType end)
        {
            var ident = Read();
            if (ident.Type != TokenType.TokenIdent)
            {
                RecoverAfterBodyItem();
                return (null, new Diagnostics(new Diagnostic(
                    severity: DiagnosticSeverity.Error,
                    summary: "Argument or block definition required",
                    detail: "An argument or block definition is required here.",
                    subject: ident.Range)));
            }

            var diags = Diagnostics.None;
            var next = Peek();
            switch (next.Type)
            {
                case TokenType.TokenEqual:
                    var (attr, attrDiags) = FinishParsingBodyAttribute(ident, false);
                    diags = diags.Append(attrDiags);
                    return (new Body(
                        new Attributes()
                        {
                            {ident.String, attr}
                        },
                        new Blocks(),
                        attr.SrcRange,
                        new Range(attr.SrcRange.Filename, attr.SrcRange.End, attr.SrcRange.End)
                    ), diags);
                case TokenType.TokenOQuote:
                case TokenType.TokenOBrace:
                case TokenType.TokenIdent:
                    RecoverAfterBodyItem();
                    
                    return (null, new Diagnostics(new Diagnostic(
                        severity: DiagnosticSeverity.Error,
                        summary: "Argument definition required",
                        detail: $"A single-line block definition can contain only a single argument. " +
                                $"If you meant to define argument {ident.String}, use an equals sign to assign it a value. " +
                                $"To define a nested block, place it on a line of its own within its parent block.",
                        subject: Range.Between(ident.Range, next.Range))));
                default:
                    RecoverAfterBodyItem();
                    
                    return (null, new Diagnostics(new Diagnostic(
                        severity: DiagnosticSeverity.Error,
                        summary: "Argument or block definition required",
                        detail: "An argument or block definition is required here. " +
                                "To set an argument, use the equals sign \"=\" to introduce the argument value.",
                        subject: ident.Range)));
            }
        }
        
        private (Attribute, Diagnostics) FinishParsingBodyAttribute(Token ident, bool singleLine)
        {
            var eqToken = Read(); // eat equals token
            if (eqToken.Type != TokenType.TokenEqual)
            {
                // should never happen if caller behaves
                throw new Exception("finishParsingBodyAttribute called with next not equals");
            }

            Range endRange;
            var (expr, diags) = ParseExpression();
            if (Recovery && diags.HasErrors)
            {
                // recovery within expressions tends to be tricky, so we've probably
                // landed somewhere weird. We'll try to reset to the start of a body
                // item so parsing can continue.
                endRange = PrevRange;
                RecoverAfterBodyItem();
            }
            else
            {
                endRange = PrevRange;
                if (!singleLine)
                {
                    var end = Peek();
                    if (end.Type != TokenType.TokenNewline && end.Type != TokenType.TokenEOF)
                    {
                        if (!Recovery)
                        {
                            var summary = "Missing newline after argument";
                            var detail = "An argument definition must end with a newline.";
                            if (end.Type == TokenType.TokenComma)
                            {
                                summary = "Unexpected comma after argument";
                                detail = "Argument definitions must be separated by newlines, not commas. " + detail;
                            }
                            
                            diags = diags.Append(new Diagnostic(
                                severity: DiagnosticSeverity.Error,
                                summary: summary,
                                detail: detail,
                                subject: Range.Between(ident.Range, end.Range)));
                        }

                        endRange = PrevRange;
                        RecoverAfterBodyItem();
                    }
                    else
                    {
                        endRange = PrevRange;
                        Read(); // eat newline
                    }
                }
            }

            return (new Attribute(ident.String, expr, Range.Between(ident.Range, endRange), ident.Range, eqToken.Range), diags);
        }

        private (INode, Diagnostics) FinishParsingBodyBlock(Token ident)
        {
            var blockType = ident.String;
            var diags = new Diagnostics();
            var labels = new List<string>();
            var labelRanges = new List<Range>();

            Token oBrace;
            while (true)
            {
                var token = Peek();
                switch (token.Type)
                {
                    case TokenType.TokenOBrace:
                        oBrace = Read();
                        goto FinishParsingBodyBlockLoopEnd;
                    case TokenType.TokenOQuote:
                        var (label, labelRange, labelDiags) = ParseQuotedStringLiteral();
                        diags = diags.Append(labelDiags);
                        labels.Add(label);
                        labelRanges.Add(labelRange);
                        // parseQuoteStringLiteral recovers up to the closing quote
                        // if it encounters problems, so we can continue looking for
                        // more labels and eventually the block body even.
                        break;
                    case TokenType.TokenIdent:
                        token = Read(); // eat token
                        labels.Add(token.String);
                        labelRanges.Add(token.Range);
                        break;
                    default:
                        string detail = "";
                        bool emitDiag = false;
                        switch (token.Type)
                        {
                            case TokenType.TokenEqual:
                                emitDiag = true;
                                detail =
                                    "The equals sign \"=\" indicates an argument definition, and must not be used when defining a block.";
                                break;
                            case TokenType.TokenNewline:
                                emitDiag = true;
                                detail =
                                    "A block definition must have block content delimited by \"{\" and \"}\", starting on the same line as the block header.";
                                break;
                            default:
                                if (!Recovery)
                                {
                                    emitDiag = true;
                                    detail =
                                        "Either a quoted string block label or an opening brace (\"{\") is expected here.";
                                }

                                break;
                        }

                        if (emitDiag)
                        {
                            diags = diags.Append(new Diagnostic(
                                severity: DiagnosticSeverity.Error,
                                summary: "Invalid block definition",
                                detail: detail,
                                subject: Range.Between(ident.Range, token.Range)));
                        }
                        RecoverAfterBodyItem();
                        return (new Block(
                                blockType, 
                                labels.ToArray(), 
                                new Body(ident.Range, ident.Range), 
                                ident.Range, 
                                labelRanges.ToArray(), 
                                ident.Range, // Placeholder 
                                ident.Range // Placeholder
                            ), diags);
                }
            }
            
            FinishParsingBodyBlockLoopEnd:
            // Once we fall out here, the peeker is pointed just after our opening
            // brace, so we can begin our nested body parsing.
            Body? body;
            Diagnostics bodyDiags;
            switch (Peek().Type)
            {
                case TokenType.TokenNewline:
                case TokenType.TokenEOF:
                case TokenType.TokenCBrace:
                    (body, bodyDiags) = ParseBody(TokenType.TokenCBrace);
                    break;
                default:
                    // Special one-line, single-attribute block parsing mode.
                    (body, bodyDiags) = ParseSingleAttrBody(TokenType.TokenCBrace);
                    switch (Peek().Type)
                    {
                        case TokenType.TokenCBrace:
                            Read(); // the happy path - just consume the closing brace
                            break;
                        case TokenType.TokenComma:
                            // User seems to be trying to use the object-constructor
                            // comma-separated style, which isn't permitted for blocks.
                        
                            diags = diags.Append(new Diagnostic(
                                severity: DiagnosticSeverity.Error,
                                summary: "Invalid single-argument block definition",
                                detail: "Single-line block syntax can include only one argument definition. To define multiple arguments, use the multi-line block syntax with one argument definition per line.",
                                subject: Peek().Range));
                            Recover(TokenType.TokenCBrace);
                            break;
                        case TokenType.TokenNewline:
                            // We don't allow weird mixtures of single and multi-line syntax.
                        
                            diags = diags.Append(new Diagnostic(
                                severity: DiagnosticSeverity.Error,
                                summary: "Invalid single-argument block definition",
                                detail: "An argument definition on the same line as its containing block creates a single-line block definition, which must also be closed on the same line. Place the block's closing brace immediately after the argument definition.",
                                subject: Peek().Range));
                            Recover(TokenType.TokenCBrace);
                            break;
                        default:
                            // Some other weird thing is going on. Since we can't guess a likely
                            // user intent for this one, we'll skip it if we're already in
                            // recovery mode.
                            if (!Recovery)
                            {
                                diags = diags.Append(new Diagnostic(
                                    severity: DiagnosticSeverity.Error,
                                    summary: "Invalid single-argument block definition",
                                    detail: "A single-line block definition must end with a closing brace immediately after its single argument definition.",
                                    subject: Peek().Range));
                            }
                            Recover(TokenType.TokenCBrace);
                            break;
                    }

                    break;
            }

            diags = diags.Append(bodyDiags);
            var cBraceRange = PrevRange;
            var eol = Peek();
            if (eol.Type == TokenType.TokenNewline || eol.Type == TokenType.TokenEOF)
            {
                Read(); // eat newline
            }
            else
            {
                if (!Recovery)
                {
                    diags = diags.Append(new Diagnostic(
                        severity: DiagnosticSeverity.Error,
                        summary: "Missing newline after block definition",
                        detail: "A block definition must end with a newline.",
                        subject: eol.Range,
                        context: Range.Between(ident.Range, eol.Range)));
                }
                RecoverAfterBodyItem();
            }
            
            // We must never produce a null body, since the caller may attempt to
            // do analysis of a partial result when there's an error, so we'll
            // insert a placeholder if we otherwise failed to produce a valid
            // body due to one of the syntax error paths above.
            if (body == null)
            {
                body = new Body(Range.Between(oBrace.Range, cBraceRange), cBraceRange);
            }

            return (new Block(
                blockType,
                labels.ToArray(),
                body,
                ident.Range,
                labelRanges.ToArray(),
                oBrace.Range,
                cBraceRange), diags);
        }

        private (Expression, Diagnostics) ParseExpression()
        {
            return ParseTernaryConditional();
        }

        private (Expression, Diagnostics) ParseTernaryConditional()
        {
            // The ternary conditional operator (.. ? .. : ..) behaves somewhat
            // like a binary operator except that the "symbol" is itself
            // an expression enclosed in two punctuation characters.
            // The middle expression is parsed as if the ? and : symbols
            // were parentheses. The "rhs" (the "false expression") is then
            // treated right-associatively so it behaves similarly to the
            // middle in terms of precedence.

            var startRange = NextRange;
            var diags = Diagnostics.None;

            var (condExpr, condDiags) = ParseBinaryOps(Operation.BinaryOps);
            diags = diags.Append(condDiags);
            if (Recovery && condDiags.HasErrors)
            {
                return (condExpr, diags);
            }

            var questionMark = Peek();
            if (questionMark.Type != TokenType.TokenQuestion)
            {
                return (condExpr, diags);
            }

            Read(); // eat question mark

            var (trueExpr, trueDiags) = ParseExpression();
            diags = diags.Append(trueDiags);
            if (Recovery && trueDiags.HasErrors)
            {
                return (condExpr, diags);
            }

            var colon = Peek();
            if (colon.Type != TokenType.TokenColon)
            {
                diags = diags.Append(new Diagnostic(
                    severity: DiagnosticSeverity.Error,
                    summary: "Missing false expression in conditional",
                    detail: "The conditional operator (...?...:...) requires a false expression, delimited by a colon.",
                    subject: colon.Range,
                    context: Range.Between(startRange, colon.Range)));
                return (condExpr, diags);
            }

            Read(); // eat colon
            
            var (falseExpr, falseDiags) = ParseExpression();
            diags = diags.Append(falseDiags);
            if (Recovery && falseDiags.HasErrors)
            {
                return (condExpr, diags);
            }

            return (new ConditionalExpression(
                condition: condExpr,
                trueResult: trueExpr,
                falseResult: falseExpr,
                srcRange: Range.Between(startRange, falseExpr.Range)),
                    diags);
        }

        /// <summary>
        /// parseBinaryOps calls itself recursively to work through all of the
        /// operator precedence groups, and then eventually calls parseExpressionTerm
        /// for each operand.
        /// </summary>
        /// <param name="binaryOps"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private (Expression, Diagnostics) ParseBinaryOps(IReadOnlyDictionary<TokenType, Operation>[] ops)
        {
            if (ops.Length == 0)
            {
                // We've run out of operators, so now we'll just try to parse a term.
                return ParseExpressionWithTraversals();
            }

            var thisLevel = ops.First();
            var remaining = ops.Skip(1).ToArray();
            var diags = Diagnostics.None;
            Expression lhs;
            Expression? rhs = null;
            Diagnostics lhsDiags, rhsDiags;

            // Parse a term that might be the first operand of a binary
            // operation or it might just be a standalone term.
            // We won't know until we've parsed it and can look ahead
            // to see if there's an operator token for this level.
            (lhs, lhsDiags) = ParseBinaryOps(remaining);
            diags = diags.Append(lhsDiags);
            if (Recovery && lhsDiags.HasErrors)
            {
                return (lhs, diags);
            }
            
            // We'll keep eating up operators until we run out, so that operators
            // with the same precedence will combine in a left-associative manner:
            // a+b+c => (a+b)+c, not a+(b+c)
            //
            // Should we later want to have right-associative operators, a way
            // to achieve that would be to call back up to ParseExpression here
            // instead of iteratively parsing only the remaining operators.
            Operation? operation = null;
            while (true)
            {
                var next = Peek();
                if (!thisLevel.TryGetValue(next.Type, out var newOp))
                {
                    break;
                }

                // Are we extending an expression started on the previous iteration?
                if (operation != null)
                {
                    lhs = new BinaryOpExpression(
                        lhs: lhs,
                        op: operation,
                        rhs: rhs!,
                        srcRange: Range.Between(lhs.Range, rhs!.Range));
                }

                operation = newOp!;
                Read(); // eat operator token
                (rhs, rhsDiags) = ParseBinaryOps(remaining);
                diags = diags.Append(rhsDiags);
                if (Recovery && rhsDiags.HasErrors)
                {
                    return (lhs, diags);
                }
            }

            if (operation == null)
            {
                return (lhs, diags);
            }
            
            return (new BinaryOpExpression(
                lhs: lhs,
                op: operation,
                rhs: rhs!,
                srcRange: Range.Between(lhs.Range, rhs!.Range)),
                    diags);
            
        }

        private (Expression, Diagnostics) ParseExpressionWithTraversals()
        {
            var (term, diags) = ParseExpressionTerm();
            var (ret, moreDiags) = ParseExpressionTraversals(term);
            diags = diags.Append(moreDiags);
            return (ret, diags);
        }

        private (Expression, Diagnostics) ParseExpressionTraversals(Expression from)
        {
            var diags = Diagnostics.None;
            var ret = from;

            while (true)
            {
                var next = Peek();
                switch (next.Type)
                {
                    case TokenType.TokenDot:
                        // Attribute access or splat
                        var dot = Read();
                        var attrToken = Peek();

                        switch (attrToken.Type)
                        {
                            case TokenType.TokenIdent:
                                attrToken = Read();
                                var name = attrToken.String;
                                var range = Range.Between(dot.Range, attrToken.Range);
                                var step = new TraverseAttr(name, range);
                                ret = ret.MakeRelativeTraversal(step, range);
                                break;
                        }
                }
            }
            
            ParseExpressionTraversalsLoopEnd:
            return (ret, diags);
        }

        private (Expression, Diagnostics) ParseExpressionTerm()
        {
            throw new NotImplementedException();
        }

        private (string, Range, Diagnostics) ParseQuotedStringLiteral()
        {
            throw new NotImplementedException();
        }


        private void RecoverAfterBodyItem()
        {
            throw new NotImplementedException();
        }

        private void Recover(TokenType end)
        {
            throw new NotImplementedException();
        }

        private Token Read()
        {
            throw new NotImplementedException();
        }

        private Token Peek()
        {
            throw new NotImplementedException();
        }
    }
}