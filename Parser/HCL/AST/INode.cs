namespace hcl_net.Parser.HCL.AST
{
    internal delegate bool WalkFunc(INode node, out INode rewritten);
    interface INode
    {
        Pos Pos { get; }
        INode Walk(WalkFunc fn);
    }
}