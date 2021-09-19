namespace hcl_net.Parse.HCL.AST
{
    internal delegate bool WalkFunc(INode node, out INode rewritten);
    interface INode
    {
        Pos Pos { get; }
        INode Walk(WalkFunc fn);
    }
}