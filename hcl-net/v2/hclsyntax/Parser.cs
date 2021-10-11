using System;
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
        public Attributes Attributes { get; }
        public Blocks Blocks { get; }
        public (BodyContent, Diagnostics) Content(BodySchema schema)
        {
            throw new System.NotImplementedException();
        }

        public (BodyContent, IBody, Diagnostics) PartialContent(BodySchema schema)
        {
            throw new System.NotImplementedException();
        }

        public (IDictionary<string, Attribute>, Diagnostics) JustAttributes()
        {
            throw new System.NotImplementedException();
        }

        public Range MissingItemRange()
        {
            throw new System.NotImplementedException();
        }

        public Range SrcRange { get; }
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