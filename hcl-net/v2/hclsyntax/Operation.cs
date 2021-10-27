using System.Collections.Generic;
using System.Collections.ObjectModel;
using cty_net;

namespace hcl_net.v2.hclsyntax
{
    public class Operation
    {
        public Operation(Function impl, IType type)
        {
            Impl = impl;
            Type = type;
        }

        public Function Impl { get; }
        public IType Type { get; }

        // TODO - implement these in the cty-net stdlib
        private static readonly Function TodoFn = new Function();
        public static readonly Operation OpLogicalOr = new(TodoFn, PrimitiveType.Bool);
        public static readonly Operation OpLogicalAnd = new(TodoFn, PrimitiveType.Bool);
        public static readonly Operation OpLogicalNot = new(TodoFn, PrimitiveType.Bool);
        public static readonly Operation OpEqual = new(TodoFn, PrimitiveType.Bool);
        public static readonly Operation OpNotEqual = new(TodoFn, PrimitiveType.Bool);
        public static readonly Operation OpGreaterThan = new(TodoFn, PrimitiveType.Bool);
        public static readonly Operation OpGreaterThanOrEqual = new(TodoFn, PrimitiveType.Bool);
        public static readonly Operation OpLessThan = new(TodoFn, PrimitiveType.Bool);
        public static readonly Operation OpLessThanOrEqual = new(TodoFn, PrimitiveType.Bool);
        
        public static readonly Operation OpAdd = new(TodoFn, PrimitiveType.Number);
        public static readonly Operation OpSubtract = new(TodoFn, PrimitiveType.Number);
        public static readonly Operation OpMultiply = new(TodoFn, PrimitiveType.Number);
        public static readonly Operation OpDivide = new(TodoFn, PrimitiveType.Number);
        public static readonly Operation OpModulo = new(TodoFn, PrimitiveType.Number);
        public static readonly Operation OpNegate = new(TodoFn, PrimitiveType.Number);

        /// <summary>
        /// This operation table maps from the operator's token type
        /// to the AST operation type. All expressions produced from
        /// binary operators are BinaryOp nodes.
        ///
        /// Binary operator groups are listed in order of precedence, with
        /// the *lowest* precedence first. Operators within the same group
        /// have left-to-right associativity.
        /// </summary>
        public static readonly IReadOnlyDictionary<TokenType, Operation>[] BinaryOps = new[]
        {
            new Dictionary<TokenType, Operation>
            {
                {TokenType.TokenOr, OpLogicalOr}
            },
            new Dictionary<TokenType, Operation>
            {
                {TokenType.TokenAnd, OpLogicalAnd}
            },
            new Dictionary<TokenType, Operation>
            {
                {TokenType.TokenEqualOp, OpEqual},
                {TokenType.TokenNotEqual, OpNotEqual}
            },
            new Dictionary<TokenType, Operation>
            {
                {TokenType.TokenGreaterThan, OpGreaterThan},
                {TokenType.TokenGreaterThanEq, OpGreaterThanOrEqual},
                {TokenType.TokenLessThan, OpLessThan},
                {TokenType.TokenLessThanEq, OpLessThanOrEqual},
            },
            new Dictionary<TokenType, Operation>
            {
                {TokenType.TokenPlus, OpAdd},
                {TokenType.TokenMinus, OpSubtract}
            },
            new Dictionary<TokenType, Operation>
            {
                {TokenType.TokenStar, OpMultiply},
                {TokenType.TokenSlash, OpDivide},
                {TokenType.TokenPercent, OpModulo}
            },
        };
    }
}