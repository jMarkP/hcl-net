using cty_net;
using hcl_net.v2.hclsyntax;

namespace hcl_net.v2
{
    /// <summary>
    /// Expression is a literal value or an expression provided in the
    /// configuration, which can be evaluated within a scope to produce a value.
    /// </summary>
    internal interface IExpression
    {
        /// <summary>
        /// Value returns the value resulting from evaluating the expression
        /// in the given evaluation context.
        ///
        /// The context may be nil, in which case the expression may contain
        /// only constants and diagnostics will be produced for any non-constant
        /// sub-expressions. (The exact definition of this depends on the source
        /// language.)
        ///
        /// The context may instead be set but have either its Variables or
        /// Functions maps set to nil, in which case only use of these features
        /// will return diagnostics.
        ///
        /// Different diagnostics are provided depending on whether the given
        /// context maps are nil or empty. In the former case, the message
        /// tells the user that variables/functions are not permitted at all,
        /// while in the latter case usage will produce a "not found" error for
        /// the specific symbol in question.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        (Value, Diagnostics) Value(EvalContext ctx);
        /// <summary>
        /// Variables returns a list of variables referenced in the receiving
        /// expression. These are expressed as absolute Traversals, so may include
        /// additional information about how the variable is used, such as
        /// attribute lookups, which the calling application can potentially use
        /// to only selectively populate the scope.
        /// </summary>
        /// <returns></returns>
        Traversal[] Variables();
        Range Range { get; }
        Range StartRange { get; }
    }
}