namespace hcl_net.v2
{
    internal class Attribute
    {
        public Attribute(string name, IExpression expression, Range range, Range nameRange)
        {
            Name = name;
            Expression = expression;
            Range = range;
            NameRange = nameRange;
        }

        public string Name { get; }
        public IExpression Expression { get; }
        public Range Range { get; }
        public Range NameRange { get; }
    }
}