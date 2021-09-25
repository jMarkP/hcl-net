namespace hcl_net.v2
{
    internal class AttributeSchema
    {
        public AttributeSchema(string name, bool required)
        {
            Name = name;
            Required = required;
        }

        public string Name { get; }
        public bool Required { get; }
    }
}