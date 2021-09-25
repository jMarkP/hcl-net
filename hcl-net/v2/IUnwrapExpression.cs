using hcl_net.v2.hclsyntax;

namespace hcl_net.v2
{
    /// <summary>
    /// Expression types can implement this if they are unwrappable
    /// </summary>
    internal interface IUnwrapExpression
    {
        /// <summary>
        /// Implementations of this method should peel away only one level
        /// of wrapping, if multiple are present. This method may return nil to
        /// indicate _dynamically_ that no wrapped expression is available, for
        /// expression types that might only behave as wrappers in certain cases.
        /// </summary>
        /// <returns></returns>
        Expression? UnwrapExpression();
    }
}