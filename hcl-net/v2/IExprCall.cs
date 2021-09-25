namespace hcl_net.v2
{
    /// <summary>
    /// An interface that an Expression type can implement if it represents
    /// a function call
    /// </summary>
    internal interface IExprCall
    {
        /// <summary>
        /// In implementing Expression types, this method should
        /// attempt to return itself as a static call (function name and expressions
        /// representing its arguments)
        /// This method should return null if a static call cannot
        /// be extracted.  Alternatively, an implementation can support
        /// UnwrapExpression to delegate handling of this function to a wrapped
        /// Expression object.
        /// </summary>
        /// <returns></returns>
        StaticCall ExprCall();
    }
}