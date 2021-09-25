using System.Collections.Generic;

namespace hcl_net.v2
{
    /// <summary>
    /// Expressions which represent maps of items should implement this
    /// </summary>
    internal interface IExprMap
    {
        /// <summary>
        /// Should return a list of expressions representing the map elements
        /// This method should return null if a static map cannot
        /// be extracted.  Alternatively, an implementation can support
        /// UnwrapExpression to delegate handling of this function to a wrapped
        /// Expression object.
        /// </summary>
        /// <returns></returns>
        IDictionary<IExpression, IExpression> ExprMap();
    }
}