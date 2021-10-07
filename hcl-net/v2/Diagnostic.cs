using System.Collections;
using System.Collections.Generic;
using System.Linq;
using hcl_net.v2.hclsyntax;

namespace hcl_net.v2
{
    internal class Diagnostic
    {
        public Diagnostic(
            DiagnosticSeverity severity,
            string? summary = default,
            string? detail = default,
            Range? subject = default,
            Range? context = default)
        {
            Severity = severity;
            Summary = summary;
            Detail = detail;
            Subject = subject;
            Context = context;
        }

        public DiagnosticSeverity Severity { get; }
        public string? Summary { get; }
        public string? Detail { get; }
        public Range? Subject { get; }

        public Range? Context { get; }
        public Expression? Expression { get; internal set; }
        public EvalContext? EvalContext { get; internal set; }
    }

    internal class Diagnostics : IEnumerable<Diagnostic>
    {
        public static readonly Diagnostics None = new ();

        public Diagnostics(Diagnostic item) : this(new[] {item})
        {
        }
        public Diagnostics(params Diagnostic[] items)
        {
            _items = items;
        }
        
        #region IEnumerable<Diagnostic> Implementation
        private readonly Diagnostic[] _items;
        public IEnumerator<Diagnostic> GetEnumerator()
        {
            return ((IEnumerable<Diagnostic>)_items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
        #endregion

        public int Length => _items.Length;
        
        public Diagnostics Append(Diagnostics other)
        {
            // Avoid needless allocations if
            // append would be a no-op
            // (This class is immutable so this is safe)
            if (other.Length == 0)
            {
                return this;
            }
            if (this.Length == 0)
            {
                return other;
            }

            return new(this.Concat(other).ToArray());
        }

        public bool HasErrors => this.Any(d => d.Severity == DiagnosticSeverity.Error);
        
        
        public void SetEvalContext(Expression expr, EvalContext ctx)
        {
            foreach (var diag in this)
            {
                if (diag.Expression == null)
                {
                    diag.Expression = expr;
                    diag.EvalContext = ctx;
                }
            }
        }
    }
}