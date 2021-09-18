using System;
using System.Collections.Generic;
using System.Linq;

namespace hcl_net.v2.hclsyntax
{
    internal static class TokenExtensions
    {
        public static bool OpensFlushHeredoc(this Token tok)
        {
            if (tok.Type != TokenType.TokenOHeredoc)
            {
                return false;
            }

            return new[] {'<', '<', '-'}.Cast<byte>().Contains(tok[0]);
        }

        public static IEnumerable<Diagnostic> CheckInvalidTokens(IEnumerable<Token> tokens)
        {
            var toldBitwise = 0;
            var toldExponent = 0;
            var toldBacktick = 0;
            var toldApostrophe = 0;
            var toldSemicolon = 0;
            var toldTabs = 0;
            var toldBadUTF8 = 0;

            foreach (var tok in tokens)
            {
                switch (tok.Type)
                {
                    case TokenType.TokenBitwiseAnd:
                    case TokenType.TokenBitwiseOr:
                    case TokenType.TokenBitwiseXor:
                    case TokenType.TokenBitwiseNot:
                        if (toldBitwise < 4)
                        {
                            string suggestion;
                            switch (tok.Type)
                            {
                                case TokenType.TokenBitwiseAnd:
                                    suggestion = " Did you mean boolean AND (\"&&\")?";
                                    break;
                                case TokenType.TokenBitwiseOr:
                                    suggestion = " Did you mean boolean OR (\"||\")?";
                                    break;
                                case TokenType.TokenBitwiseNot:
                                    suggestion = " Did you mean boolean NOT (\"!\")?";
                                    break;
                                default:
                                    suggestion = String.Empty;
                                    break;
                            }

                            yield return new Diagnostic(
                                severity: DiagnosticSeverity.Error,
                                summary: "Unsupported operator",
                                detail: $"Bitwise operators are not supported.{suggestion}",
                                subject: tok.Range
                            );
                            toldBitwise++;
                        }

                        break;
                    case TokenType.TokenStarStar:
                        if (toldExponent < 1)
                        {
                            yield return new Diagnostic(
                                severity: DiagnosticSeverity.Error,
                                summary: "Unsupported operator",
                                detail:
                                "\"**\" is not a supported operator. Exponentiation is not supported as an operator.",
                                subject: tok.Range
                            );

                            toldExponent++;
                        }

                        break;
                    case TokenType.TokenBacktick:
                        // Only report for alternating (even) backticks, so we won't report both start and ends of the same
                        // backtick-quoted string.
                        if ((toldBacktick % 2) == 0)
                        {
                            yield return new Diagnostic(
                                severity: DiagnosticSeverity.Error,
                                summary: "Invalid character",
                                detail:
                                "The \"`\" character is not valid. To create a multi-line string, use the \"heredoc\" syntax, like \"<<EOT\".",
                                subject: tok.Range
                            );
                        }

                        if (toldBacktick <= 2)
                        {
                            toldBacktick++;
                        }

                        break;
                    case TokenType.TokenApostrophe:
                        if ((toldApostrophe % 2) == 0)
                        {
                            yield return new Diagnostic(
                                severity: DiagnosticSeverity.Error,
                                summary: "Invalid character",
                                detail: "Single quotes are not valid. Use double quotes (\") to enclose strings.",
                                subject: tok.Range
                            );
                        }

                        if (toldApostrophe <= 2)
                        {
                            toldApostrophe++;
                        }

                        break;
                    case TokenType.TokenSemicolon:
                        if (toldSemicolon < 1)
                        {
                            yield return new Diagnostic(
                                severity: DiagnosticSeverity.Error,
                                summary: "Invalid character",
                                detail:
                                "The \";\" character is not valid. Use newlines to separate arguments and blocks, and commas to separate items in collection values.",
                                subject: tok.Range
                            );

                            toldSemicolon++;
                        }

                        break;
                    case TokenType.TokenTabs:
                        if (toldTabs < 1)
                        {
                            yield return new Diagnostic(
                                severity: DiagnosticSeverity.Error,
                                summary: "Invalid character",
                                detail:
                                "Tab characters may not be used. The recommended indentation style is two spaces per indent.",
                                subject: tok.Range
                            );

                            toldTabs++;
                        }

                        break;
                    case TokenType.TokenBadUTF8:
                        if (toldBadUTF8 < 1)
                        {
                            yield return new Diagnostic(
                                severity: DiagnosticSeverity.Error,
                                summary: "Invalid character encoding",
                                detail:
                                "All input files must be UTF-8 encoded. Ensure that UTF-8 encoding is selected in your editor.",
                                subject: tok.Range
                            );

                            toldBadUTF8++;
                        }

                        break;
                    case TokenType.TokenQuotedNewline:
                        yield return new Diagnostic(
                            severity: DiagnosticSeverity.Error,
                            summary: "Invalid multi-line string",
                            detail:
                            "Quoted strings may not be split over multiple lines. To produce a multi-line string, either use the \\n escape to represent a newline character or use the \"heredoc\" multi-line template syntax.",
                            subject: tok.Range
                        );
                        break;
                    case TokenType.TokenInvalid:
                        if (tok.Range.Length == 1 && new[] {'“', '”'}.Cast<byte>().Contains(tok[0]))
                        {
                            yield return new Diagnostic(
                                severity: DiagnosticSeverity.Error,
                                summary: "Invalid character",
                                detail:
                                "\"Curly quotes\" are not valid here. These can sometimes be inadvertently introduced when sharing code via documents or discussion forums. It might help to replace the character with a \"straight quote\".",
                                subject: tok.Range
                            );
                        }
                        else
                        {
                            yield return new Diagnostic(
                                severity: DiagnosticSeverity.Error,
                                summary: "Invalid character",
                                detail: "This character is not used within the language.",
                                subject: tok.Range
                            );
                        }
                        break;
                }
            }
        }

        private static byte[] Utf8BOM = {0xef, 0xbb, 0xbf};

        // stripUTF8BOM checks whether the given buffer begins with a UTF-8 byte order
        // mark (0xEF 0xBB 0xBF) and, if so, returns a truncated slice with the same
        // backing array but with the BOM skipped.
        //
        // If there is no BOM present, the given slice is returned verbatim.
        public static byte[] StripUTF8BOM(this byte []src)
        {
            if (src.Length < 3)
            {
                return src;
            }
            var hasPrefix = true;
            for (var i = 0; i < Utf8BOM.Length; i++)
            {
                if (src[i] != Utf8BOM[i])
                {
                    hasPrefix = false;
                    break;
                }
            }

            if (hasPrefix)
            {
                return src[3..];
            }

            return src;
        }
    }
}