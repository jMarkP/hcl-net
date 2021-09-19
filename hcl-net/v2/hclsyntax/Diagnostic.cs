namespace hcl_net.v2.hclsyntax
{
    internal readonly struct Diagnostic
    {
        public Diagnostic(
            DiagnosticSeverity severity,
            string summary = default,
            string detail = default,
            TokenRange? subject = default,
            TokenRange? context = default)
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
        public TokenRange? Subject { get; }

        public TokenRange? Context { get; }
        // public Expression Expression { get; }
        // public EvalContext EvalContext { get; }
    }
}