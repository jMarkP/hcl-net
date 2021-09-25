using hcl_net.v2.hclsyntax;

namespace hcl_net.v2
{
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