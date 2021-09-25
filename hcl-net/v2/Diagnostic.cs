using hcl_net.v2.hclsyntax;

namespace hcl_net.v2
{
    internal readonly struct Diagnostic
    {
        public Diagnostic(
            DiagnosticSeverity severity,
            string summary = default,
            string detail = default,
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
        public string Summary { get; }
        public string Detail { get; }
        public Range? Subject { get; }

        public Range? Context { get; }
        // public Expression Expression { get; }
        // public EvalContext EvalContext { get; }
    }
}