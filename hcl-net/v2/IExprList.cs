namespace hcl_net.v2
{
    /// <summary>
    /// Expressions which represent lists of items should implement this
    /// </summary>
    internal interface IExprList
    {
        /// <summary>
        /// Should return a list of expressions representing the items
        /// in the list
        /// This method should return null if a static list cannot
        /// be extracted.  Alternatively, an implementation can support
        /// UnwrapExpression to delegate handling of this function to a wrapped
        /// Expression object.
        /// </summary>
        /// <returns></returns>
        IExpression[] ExprList();
    }
}