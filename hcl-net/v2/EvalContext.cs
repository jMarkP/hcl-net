using System.Collections.Generic;
using cty_net;

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
}