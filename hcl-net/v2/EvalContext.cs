using System.Collections.Generic;
using cty_net;
using hcl_net.v2.hclsyntax;

namespace hcl_net.v2
{
    /// <summary>
    /// An EvalContext provides the variables and functions that should be used
    /// to evaluate an expression.
    /// </summary>
    internal class EvalContext
    {
        public EvalContext()
        {
        }

        private EvalContext(EvalContext parent)
        {
            Parent = parent;
        }

        public EvalContext NewChild()
        {
            return new EvalContext(this);
        }
        
        public Dictionary<string, Value> Variables { get; } = new();
        public Dictionary<string, Function> Functions { get; } = new ();
        public EvalContext? Parent { get; }
    }

    internal class StaticCall
    {
        public StaticCall(string name, Range nameRange, Expression[] arguments, Range argsRange)
        {
            Name = name;
            NameRange = nameRange;
            Arguments = arguments;
            ArgsRange = argsRange;
        }

        public string Name { get; }
        public Range NameRange { get; }
        public Expression[] Arguments { get; }
        public Range ArgsRange { get; }
    }
}