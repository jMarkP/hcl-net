namespace hcl_net.v2.hclsyntax.parser.AST
{
    internal delegate bool WalkFunc(INode node, out INode rewritten);
    interface INode
    {
        Pos Pos { get; }
        INode Walk(WalkFunc fn);
    }
}