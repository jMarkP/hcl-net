using cty_net;

namespace hcl_net.v2
{
    internal interface ITraverser
    {
        (Value, Diagnostics) TraversalStep(Value val);
        Range SourceRange { get; }
    }
}