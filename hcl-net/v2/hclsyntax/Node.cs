using System;
using System.Collections.Generic;
using System.Linq;
using cty_net;

namespace hcl_net.v2.hclsyntax
{
    using InternalWalkFunc = Action<Node>;
    
    internal abstract class Node
    {
        protected Node(Range range)
        {
            Range = range;
        }

        public Range Range { get; }

        public abstract void WalkChildNodes(InternalWalkFunc func);
    }

    internal abstract class Expression : Node, IExpression
    {
        protected Expression(Range range) : base(range)
        {
            StartRange = range;
        }
        
        public abstract (Value, Diagnostic[]?) Value(EvalContext ctx);

        public Traversal[] Variables()
        {
            var result = new List<Traversal>();
            var walker = new VariablesWalker((Traversal t) => result.Add(t));
            walker.Walk(this);

            return result.ToArray();
        }
        public virtual Range StartRange { get; }
    }

    internal class VariablesWalker : IWalker
    {
        private Action<Traversal> _callback;
        private List<Dictionary<string, object>> _localScopes = new ();

        public VariablesWalker(Action<Traversal> callback)
        {
            _callback = callback;
        }

        public IEnumerable<Diagnostic>? Enter(Node node)
        {
            if (node is ScopeTraversalExpression scopeTraversal)
            {
                var t = scopeTraversal.Traversal;
                
                // Check if the given root name appears in any of the active
                // local scopes. We don't want to return local variables here, since
                // the goal of walking variables is to tell the calling application
                // which names it needs to populate in the _root_ scope.
                var name = t.RootName;
                if (_localScopes.Any(s => s.ContainsKey(name)))
                {
                    return null;
                }

                _callback(t);
            }
            else if (node is ChildScope childScope)
            {
                _localScopes.Add(childScope.LocalNames);
            }

            return null;
        }

        public IEnumerable<Diagnostic>? Exit(Node node)
        {
            if (node is ChildScope childScope)
            {
                _localScopes.RemoveAt(_localScopes.Count - 1);
            }

            return null;
        }
    }

    /// <summary>
    /// ChildScope is a synthetic AST node that is visited during a walk to
    /// indicate that its descendent will be evaluated in a child scope, which
    /// may mask certain variables from the parent scope as locals.
    ///
    /// ChildScope nodes don't really exist in the AST, but are rather synthesized
    /// on the fly during walk. Therefore it doesn't do any good to transform them;
    /// instead, transform either parent node that created a scope or the expression
    /// that the child scope struct wraps.
    /// </summary>
    internal class ChildScope : Expression
    {
        public Dictionary<string, object> LocalNames { get; }
        public Expression Expr { get; }
        public ChildScope(Expression expr, Dictionary<string, object> localNames) : base(expr.Range)
        {
            Expr = expr;
            LocalNames = localNames;
        }

        public override void WalkChildNodes(InternalWalkFunc func)
        {
            func(Expr);
        }

        public override (Value, Diagnostic[]?) Value(EvalContext ctx)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// ScopeTraversalExpr is an Expression that retrieves a value from the scope
    /// using a traversal.
    /// </summary>
    internal class ScopeTraversalExpression : Expression
    {
        public Traversal Traversal { get; }

        public ScopeTraversalExpression(Range range, Traversal traversal) : base(range)
        {
            Traversal = traversal;
        }

        public override void WalkChildNodes(InternalWalkFunc func)
        {
            // Scope traversals have no child nodes
        }

        public override (Value, Diagnostic[]?) Value(EvalContext ctx)
        {
            var (val, diags) = Traversal.TraverseAbs(ctx);
            setDiagEvalContext(diags, this, ctx);
            return (val, diags);
        }
    }
    
    internal class ParenthesesExpression : Expression
    {
        public ParenthesesExpression(Range range, Expression inner) : base(range)
        {
            Inner = inner;
        }
        
        public Expression Inner { get; }

        public override void WalkChildNodes(InternalWalkFunc func)
        {
            func(Inner);
        }

        public override (Value, Diagnostic[]?) Value(EvalContext ctx)
        {
            throw new NotImplementedException();
        }
    }

    internal class LiteralValueExpression : Expression
    {
        public LiteralValueExpression(Range range, Value value) : base(range)
        {
            this.Val = value;
        }
        
        public Value Val { get; }

        public override void WalkChildNodes(InternalWalkFunc func)
        {
            // Literal values have no child nodes
        }

        public override (Value, Diagnostic[]?) Value(EvalContext ctx)
        {
            return (Val, null);
        }
    }

    internal static class NodeWalkerExtensions
    {
        public static IEnumerable<Diagnostic> Walk(this IWalker walker, Node node)
        {
            var diags = walker.Enter(node);
            node.WalkChildNodes(n =>
            {
                // ReSharper disable once AccessToModifiedClosure
                diags = diags.Concat(walker.Walk(n));
            });
            diags = diags.Concat(walker.Exit(node));
            return diags;
        }
    }

    internal interface IWalker
    {
        IEnumerable<Diagnostic>? Enter(Node node);
        IEnumerable<Diagnostic>? Exit(Node node);
    }
}