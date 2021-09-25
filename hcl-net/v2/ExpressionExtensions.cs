using System;
using System.Collections.Generic;
using hcl_net.v2.hclsyntax;

namespace hcl_net.v2
{
    internal static class ExpressionExtensions
    {
        public static (StaticCall?, Diagnostic[]?) ExprCall(this IExpression expr)
        {
            return Transform(expr, (IExprCall x) => x.ExprCall(), "A static function call is required.");
        }

        public static (IExpression[]?, Diagnostic[]?) ExprList(this IExpression expr)
        {
            return Transform(expr, (IExprList x) => x.ExprList(), "A static list expression is required.");
        }

        public static (IDictionary<IExpression, IExpression>?, Diagnostic[]?) ExprMap(this IExpression expr)
        {
            return Transform(expr, (IExprMap x) => x.ExprMap(), "A static map expression is required");
        }

        private static (TOut?, Diagnostic[]?) Transform<TImpl, TOut>(this IExpression expr, Func<TImpl, TOut> map, string errorDetail) 
            where TImpl : class
            where TOut : class
        {
            var impl = expr.UnwrapExpressionUntilType<TImpl>();
            if (impl != null)
            {
                return (map(impl), null);
            }

            return (null, new[]
            {
                new Diagnostic(
                    DiagnosticSeverity.Error,
                    "Invalid expression",
                    errorDetail,
                    expr.StartRange)
            });
        }
        
        /// <summary>
        /// UnwrapExpression removes any "wrapper" expressions from the given expression,
        /// to recover the representation of the physical expression given in source
        /// code.
        ///
        /// Sometimes wrapping expressions are used to modify expression behavior, e.g.
        /// in extensions that need to make some local variables available to certain
        /// sub-trees of the configuration. This can make it difficult to reliably
        /// type-assert on the physical AST types used by the underlying syntax.
        ///
        /// Unwrapping an expression may modify its behavior by stripping away any
        /// additional constraints or capabilities being applied to the Value and
        /// Variables methods, so this function should generally only be used prior
        /// to operations that concern themselves with the static syntax of the input
        /// configuration, and not with the effective value of the expression.
        ///
        /// Wrapper expression types must support unwrapping by
        /// implementing <see cref="IUnwrapExpression"/>
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static IExpression UnwrapExpression(this IExpression expr)
        {
            while (true)
            {
                if (!(expr is IUnwrapExpression unwrap))
                {
                    return expr;
                }

                var innerExpr = unwrap.UnwrapExpression();
                if (innerExpr == null)
                {
                    return expr;
                }

                expr = innerExpr;
            }
        }

        public static IExpression? UnwrapExpressionUntil(this IExpression expr, Func<IExpression, bool> until)
        {
            IExpression? e = expr;
            while (true)
            {
                if (until(e))
                {
                    return expr;
                }

                if (!(e is IUnwrapExpression unwrap))
                {
                    return null;
                }

                e = unwrap.UnwrapExpression();
                if (e == null)
                {
                    return null;
                }
            }
        }

        public static T? UnwrapExpressionUntilType<T>(this IExpression expr) where T : class
        {
            var found = expr.UnwrapExpressionUntil(e => e is T);
            return found == null ? (T?)null : (T)found;
        }
    }
}