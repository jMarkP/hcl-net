using System.Linq;
using System.Text;
using hcl_net.v2;
using hcl_net.v2.hclsyntax;
using NUnit.Framework;

namespace hcl_net.Test.v2.hclsyntax
{
    [TestFixture]
    public class ScanTokensTest
    {

        [TestCaseSource(nameof(ScannerTestCases))]
        public void TestScanTokens(TestCase testCase)
        {
	        var bytes = testCase.InputString != null
		        ? Encoding.UTF8.GetBytes(testCase.InputString)
		        : testCase.Input;
	        var actual = Scanner.ScanTokens(bytes,
		        "", Pos.CreateForFile(""), ScanMode.Normal).ToArray();
	        
	        CompareTokens(actual, testCase.ExpectedTokens);
        }
        
        
        [TestCaseSource(nameof(TemplateTestCases))]
        public void TestScanTokens_Template(TestCase testCase)
        {
	        var bytes = testCase.InputString != null
		        ? Encoding.UTF8.GetBytes(testCase.InputString)
		        : testCase.Input;
	        var actual = Scanner.ScanTokens(bytes,
		        "", Pos.CreateForFile(""), ScanMode.Template).ToArray();
	        
	        CompareTokens(actual, testCase.ExpectedTokens);
        }

        private void CompareTokens(Token[] actual, ExpectedToken[] expected)
        {
	        var actualString = string.Join(", ", actual.Select(t =>
		        $"[{t.Type}] `{string.Join("", t.GetBytes().ToArray().Select(b => b.ToString("X")))}`"));
	        
	        var expectedString = string.Join(", ", expected.Select(t =>
		        $"[{t.Type}] `{string.Join("", t.ExpectedBytes.Select(b => b.ToString("X")))}`"));
	        
	        Assert.That(actualString, Is.EqualTo(expectedString));
        }

        private void CompareToken(Token actual, ExpectedToken expected)
        {
	        Assert.That(actual.GetBytes().ToArray(), Is.EquivalentTo(expected.ExpectedBytes));
	        ComparePos(actual.Range.Start, expected.ExpectedRange.Start);
	        ComparePos(actual.Range.End, expected.ExpectedRange.End);
        }

        private void ComparePos(Pos actual, (int Byte, int Line, int Column) expected)
        {
	        Assert.That(actual.Byte, Is.EqualTo(expected.Byte));
	        // Assert.That(actual.Line, Is.EqualTo(expected.Line));
	        // Assert.That(actual.Column, Is.EqualTo(expected.Column));
        }

        private static TestCaseData[] ScannerTestCases()
        {
	        return new[]
	        {
		        // Empty input
		        new TestCase
		        {
			        InputString = "",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 0, Line: 1, Column: 1),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = " ",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = "\n\n",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNewline,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 2, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNewline,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 2, Column: 1),
						        End = (Byte: 2, Line: 3, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 2, Line: 3, Column: 1),
						        End = (Byte: 2, Line: 3, Column: 1),
					        },
				        },
			        },
		        },

		        // Byte-order mark
		        new TestCase
		        {
			        Input = new byte[] {0xef, 0xbb, 0xbf}, // Leading UTF-8 byte-order mark is ignored...
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange // ...but its bytes still count when producing ranges
					        {
						        Start = (Byte: 3, Line: 1, Column: 1),
						        End = (Byte: 3, Line: 1, Column: 1),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        Input = new byte[] { 0x20, 0xef, 0xbb, 0xbf }, // Non-leading BOM is invalid
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenInvalid,
					        ExpectedBytes = TokenExtensions.Utf8BOM,
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 4, Line: 1, Column: 3),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 4, Line: 1, Column: 3),
						        End = (Byte: 4, Line: 1, Column: 3),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        Input = new byte[] { 0xfe, 0xff }, // UTF-16 BOM is invalid
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenBadUTF8,
					        ExpectedBytes = new byte[] {0xfe},
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenBadUTF8,
					        ExpectedBytes = new byte[] {0xff},
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 2, Line: 1, Column: 3),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 2, Line: 1, Column: 3),
						        End = (Byte: 2, Line: 1, Column: 3),
					        },
				        },
			        },
		        },

		        // TokenNumberLit
		        new TestCase
		        {
			        InputString = "1",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNumberLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("1"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = "12",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNumberLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("12"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 2, Line: 1, Column: 3),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 2, Line: 1, Column: 3),
						        End = (Byte: 2, Line: 1, Column: 3),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = "12.3",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNumberLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("12.3"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 4, Line: 1, Column: 5),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 4, Line: 1, Column: 5),
						        End = (Byte: 4, Line: 1, Column: 5),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = "1e2",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNumberLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("1e2"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 3, Line: 1, Column: 4),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 3, Line: 1, Column: 4),
						        End = (Byte: 3, Line: 1, Column: 4),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = "1e+2",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNumberLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("1e+2"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 4, Line: 1, Column: 5),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 4, Line: 1, Column: 5),
						        End = (Byte: 4, Line: 1, Column: 5),
					        },
				        },
			        },
		        },

		        // TokenIdent
		        new TestCase
		        {
			        InputString = "hello",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenIdent,
					        ExpectedBytes = Encoding.UTF8.GetBytes("hello"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 5, Line: 1, Column: 6),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 5, Line: 1, Column: 6),
						        End = (Byte: 5, Line: 1, Column: 6),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = "_ello",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenIdent,
					        ExpectedBytes = Encoding.UTF8.GetBytes("_ello"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 5, Line: 1, Column: 6),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 5, Line: 1, Column: 6),
						        End = (Byte: 5, Line: 1, Column: 6),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = "hel_o",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenIdent,
					        ExpectedBytes = Encoding.UTF8.GetBytes("hel_o"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 5, Line: 1, Column: 6),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 5, Line: 1, Column: 6),
						        End = (Byte: 5, Line: 1, Column: 6),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = "hel-o",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenIdent,
					        ExpectedBytes = Encoding.UTF8.GetBytes("hel-o"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 5, Line: 1, Column: 6),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 5, Line: 1, Column: 6),
						        End = (Byte: 5, Line: 1, Column: 6),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = "h3ll0",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenIdent,
					        ExpectedBytes = Encoding.UTF8.GetBytes("h3ll0"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 5, Line: 1, Column: 6),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 5, Line: 1, Column: 6),
						        End = (Byte: 5, Line: 1, Column: 6),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = "héllo", // combining acute accent
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenIdent,
					        ExpectedBytes = Encoding.UTF8.GetBytes("héllo"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 7, Line: 1, Column: 6),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 7, Line: 1, Column: 6),
						        End = (Byte: 7, Line: 1, Column: 6),
					        },
				        },
			        },
		        },

		        // Literal-only Templates (string literals, effectively)
		        new TestCase
		        {
			        InputString = @"""""",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 2, Line: 1, Column: 3),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 2, Line: 1, Column: 3),
						        End = (Byte: 2, Line: 1, Column: 3),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = @"""hello""",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("hello"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 6, Line: 1, Column: 7),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 6, Line: 1, Column: 7),
						        End = (Byte: 7, Line: 1, Column: 8),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 7, Line: 1, Column: 8),
						        End = (Byte: 7, Line: 1, Column: 8),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = @"""hello, \""world\""!""",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes =
						        Encoding.UTF8.GetBytes(
							        @"hello, \""world\""!"), // The escapes are handled by the parser, not the scanner
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 18, Line: 1, Column: 19),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 18, Line: 1, Column: 19),
						        End = (Byte: 19, Line: 1, Column: 20),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 19, Line: 1, Column: 20),
						        End = (Byte: 19, Line: 1, Column: 20),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = @"""hello $$""",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("hello "),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 7, Line: 1, Column: 8),
					        },
				        },
				        // This one scans a little oddly because of how the scanner
				        // handles the escaping of the dollar sign, but it's still
				        // good enough for the parser since it'll just concatenate
				        // these two string literals together anyway.
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("$"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 7, Line: 1, Column: 8),
						        End = (Byte: 8, Line: 1, Column: 9),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("$"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 8, Line: 1, Column: 9),
						        End = (Byte: 9, Line: 1, Column: 10),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 9, Line: 1, Column: 10),
						        End = (Byte: 10, Line: 1, Column: 11),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 10, Line: 1, Column: 11),
						        End = (Byte: 10, Line: 1, Column: 11),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = @"""hello %%""",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("hello "),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 7, Line: 1, Column: 8),
					        },
				        },
				        // This one scans a little oddly because of how the scanner
				        // handles the escaping of the percent sign, but it's still
				        // good enough for the parser since it'll just concatenate
				        // these two string literals together anyway.
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("%"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 7, Line: 1, Column: 8),
						        End = (Byte: 8, Line: 1, Column: 9),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("%"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 8, Line: 1, Column: 9),
						        End = (Byte: 9, Line: 1, Column: 10),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 9, Line: 1, Column: 10),
						        End = (Byte: 10, Line: 1, Column: 11),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 10, Line: 1, Column: 11),
						        End = (Byte: 10, Line: 1, Column: 11),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = @"""hello $""",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("hello "),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 7, Line: 1, Column: 8),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("$"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 7, Line: 1, Column: 8),
						        End = (Byte: 8, Line: 1, Column: 9),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 8, Line: 1, Column: 9),
						        End = (Byte: 9, Line: 1, Column: 10),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 9, Line: 1, Column: 10),
						        End = (Byte: 9, Line: 1, Column: 10),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = @"""hello %""",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("hello "),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 7, Line: 1, Column: 8),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("%"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 7, Line: 1, Column: 8),
						        End = (Byte: 8, Line: 1, Column: 9),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 8, Line: 1, Column: 9),
						        End = (Byte: 9, Line: 1, Column: 10),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 9, Line: 1, Column: 10),
						        End = (Byte: 9, Line: 1, Column: 10),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = @"""hello $${world}""",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("hello "),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 7, Line: 1, Column: 8),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("$${"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 7, Line: 1, Column: 8),
						        End = (Byte: 10, Line: 1, Column: 11),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("world}"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 10, Line: 1, Column: 11),
						        End = (Byte: 16, Line: 1, Column: 17),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 16, Line: 1, Column: 17),
						        End = (Byte: 17, Line: 1, Column: 18),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 17, Line: 1, Column: 18),
						        End = (Byte: 17, Line: 1, Column: 18),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = @"""hello %%{world}""",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("hello "),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 7, Line: 1, Column: 8),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("%%{"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 7, Line: 1, Column: 8),
						        End = (Byte: 10, Line: 1, Column: 11),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("world}"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 10, Line: 1, Column: 11),
						        End = (Byte: 16, Line: 1, Column: 17),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 16, Line: 1, Column: 17),
						        End = (Byte: 17, Line: 1, Column: 18),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 17, Line: 1, Column: 18),
						        End = (Byte: 17, Line: 1, Column: 18),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = @"""hello %${world}""",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("hello "),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 7, Line: 1, Column: 8),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("%"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 7, Line: 1, Column: 8),
						        End = (Byte: 8, Line: 1, Column: 9),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenTemplateInterp,
					        ExpectedBytes = Encoding.UTF8.GetBytes("${"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 8, Line: 1, Column: 9),
						        End = (Byte: 10, Line: 1, Column: 11),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenIdent,
					        ExpectedBytes = Encoding.UTF8.GetBytes("world"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 10, Line: 1, Column: 11),
						        End = (Byte: 15, Line: 1, Column: 16),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenTemplateSeqEnd,
					        ExpectedBytes = Encoding.UTF8.GetBytes("}"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 15, Line: 1, Column: 16),
						        End = (Byte: 16, Line: 1, Column: 17),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 16, Line: 1, Column: 17),
						        End = (Byte: 17, Line: 1, Column: 18),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 17, Line: 1, Column: 18),
						        End = (Byte: 17, Line: 1, Column: 18),
					        },
				        },
			        },
		        },

		        // Templates with interpolations and control sequences
		        new TestCase
		        {
			        InputString = @"""${1}""",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenTemplateInterp,
					        ExpectedBytes = Encoding.UTF8.GetBytes("${"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 3, Line: 1, Column: 4),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNumberLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("1"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 3, Line: 1, Column: 4),
						        End = (Byte: 4, Line: 1, Column: 5),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenTemplateSeqEnd,
					        ExpectedBytes = Encoding.UTF8.GetBytes("}"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 4, Line: 1, Column: 5),
						        End = (Byte: 5, Line: 1, Column: 6),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 5, Line: 1, Column: 6),
						        End = (Byte: 6, Line: 1, Column: 7),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 6, Line: 1, Column: 7),
						        End = (Byte: 6, Line: 1, Column: 7),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = @"""%{a}""",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenTemplateControl,
					        ExpectedBytes = Encoding.UTF8.GetBytes("%{"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 3, Line: 1, Column: 4),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenIdent,
					        ExpectedBytes = Encoding.UTF8.GetBytes("a"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 3, Line: 1, Column: 4),
						        End = (Byte: 4, Line: 1, Column: 5),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenTemplateSeqEnd,
					        ExpectedBytes = Encoding.UTF8.GetBytes("}"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 4, Line: 1, Column: 5),
						        End = (Byte: 5, Line: 1, Column: 6),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 5, Line: 1, Column: 6),
						        End = (Byte: 6, Line: 1, Column: 7),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 6, Line: 1, Column: 7),
						        End = (Byte: 6, Line: 1, Column: 7),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = @"""${{}}""",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenTemplateInterp,
					        ExpectedBytes = Encoding.UTF8.GetBytes("${"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 3, Line: 1, Column: 4),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOBrace,
					        ExpectedBytes = Encoding.UTF8.GetBytes("{"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 3, Line: 1, Column: 4),
						        End = (Byte: 4, Line: 1, Column: 5),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCBrace,
					        ExpectedBytes = Encoding.UTF8.GetBytes("}"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 4, Line: 1, Column: 5),
						        End = (Byte: 5, Line: 1, Column: 6),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenTemplateSeqEnd,
					        ExpectedBytes = Encoding.UTF8.GetBytes("}"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 5, Line: 1, Column: 6),
						        End = (Byte: 6, Line: 1, Column: 7),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 6, Line: 1, Column: 7),
						        End = (Byte: 7, Line: 1, Column: 8),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 7, Line: 1, Column: 8),
						        End = (Byte: 7, Line: 1, Column: 8),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = @"""${""""}""",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenTemplateInterp,
					        ExpectedBytes = Encoding.UTF8.GetBytes("${"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 3, Line: 1, Column: 4),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 3, Line: 1, Column: 4),
						        End = (Byte: 4, Line: 1, Column: 5),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 4, Line: 1, Column: 5),
						        End = (Byte: 5, Line: 1, Column: 6),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenTemplateSeqEnd,
					        ExpectedBytes = Encoding.UTF8.GetBytes("}"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 5, Line: 1, Column: 6),
						        End = (Byte: 6, Line: 1, Column: 7),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 6, Line: 1, Column: 7),
						        End = (Byte: 7, Line: 1, Column: 8),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 7, Line: 1, Column: 8),
						        End = (Byte: 7, Line: 1, Column: 8),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = @"""${""${a}""}""",
		        ExpectedTokens = new[]
		        {
			        new ExpectedToken
			        {
				        Type = TokenType.TokenOQuote,
				        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
				        ExpectedRange = new ExpectedRange
				        {
					        Start = (Byte: 0, Line: 1, Column: 1),
					        End = (Byte: 1, Line: 1, Column: 2),
				        },
			        },
			        new ExpectedToken
			        {
				        Type = TokenType.TokenTemplateInterp,
				        ExpectedBytes = Encoding.UTF8.GetBytes("${"),
				        ExpectedRange = new ExpectedRange
				        {
					        Start = (Byte: 1, Line: 1, Column: 2),
					        End = (Byte: 3, Line: 1, Column: 4),
				        },
			        },
			        new ExpectedToken
			        {
				        Type = TokenType.TokenOQuote,
				        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
				        ExpectedRange = new ExpectedRange
				        {
					        Start = (Byte: 3, Line: 1, Column: 4),
					        End = (Byte: 4, Line: 1, Column: 5),
				        },
			        },
			        new ExpectedToken
			        {
				        Type = TokenType.TokenTemplateInterp,
				        ExpectedBytes = Encoding.UTF8.GetBytes("${"),
				        ExpectedRange = new ExpectedRange
				        {
					        Start = (Byte: 4, Line: 1, Column: 5),
					        End = (Byte: 6, Line: 1, Column: 7),
				        },
			        },
			        new ExpectedToken
			        {
				        Type = TokenType.TokenIdent,
				        ExpectedBytes = Encoding.UTF8.GetBytes("a"),
				        ExpectedRange = new ExpectedRange
				        {
					        Start = (Byte: 6, Line: 1, Column: 7),
					        End = (Byte: 7, Line: 1, Column: 8),
				        },
			        },
			        new ExpectedToken
			        {
				        Type = TokenType.TokenTemplateSeqEnd,
				        ExpectedBytes = Encoding.UTF8.GetBytes("}"),
				        ExpectedRange = new ExpectedRange
				        {
					        Start = (Byte: 7, Line: 1, Column: 8),
					        End = (Byte: 8, Line: 1, Column: 9),
				        },
			        },
			        new ExpectedToken
			        {
				        Type = TokenType.TokenCQuote,
				        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
				        ExpectedRange = new ExpectedRange
				        {
					        Start = (Byte: 8, Line: 1, Column: 9),
					        End = (Byte: 9, Line: 1, Column: 10),
				        },
			        },
			        new ExpectedToken
			        {
				        Type = TokenType.TokenTemplateSeqEnd,
				        ExpectedBytes = Encoding.UTF8.GetBytes("}"),
				        ExpectedRange = new ExpectedRange
				        {
					        Start = (Byte: 9, Line: 1, Column: 10),
					        End = (Byte: 10, Line: 1, Column: 11),
				        },
			        },
			        new ExpectedToken
			        {
				        Type = TokenType.TokenCQuote,
				        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
				        ExpectedRange = new ExpectedRange
				        {
					        Start = (Byte: 10, Line: 1, Column: 11),
					        End = (Byte: 11, Line: 1, Column: 12),
				        },
			        },
			        new ExpectedToken
			        {
				        Type = TokenType.TokenEOF,
				        ExpectedBytes = new byte[] { },
				        ExpectedRange = new ExpectedRange
				        {
					        Start = (Byte: 11, Line: 1, Column: 12),
					        End = (Byte: 11, Line: 1, Column: 12),
				        },
			        },
		        },
		        },
		        new TestCase
		        {
			        InputString = @"""${""${a} foo""}""",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenTemplateInterp,
					        ExpectedBytes = Encoding.UTF8.GetBytes("${"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 3, Line: 1, Column: 4),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 3, Line: 1, Column: 4),
						        End = (Byte: 4, Line: 1, Column: 5),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenTemplateInterp,
					        ExpectedBytes = Encoding.UTF8.GetBytes("${"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 4, Line: 1, Column: 5),
						        End = (Byte: 6, Line: 1, Column: 7),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenIdent,
					        ExpectedBytes = Encoding.UTF8.GetBytes("a"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 6, Line: 1, Column: 7),
						        End = (Byte: 7, Line: 1, Column: 8),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenTemplateSeqEnd,
					        ExpectedBytes = Encoding.UTF8.GetBytes("}"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 7, Line: 1, Column: 8),
						        End = (Byte: 8, Line: 1, Column: 9),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes(" foo"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 8, Line: 1, Column: 9),
						        End = (Byte: 12, Line: 1, Column: 13),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 12, Line: 1, Column: 13),
						        End = (Byte: 13, Line: 1, Column: 14),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenTemplateSeqEnd,
					        ExpectedBytes = Encoding.UTF8.GetBytes("}"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 13, Line: 1, Column: 14),
						        End = (Byte: 14, Line: 1, Column: 15),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 14, Line: 1, Column: 15),
						        End = (Byte: 15, Line: 1, Column: 16),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 15, Line: 1, Column: 16),
						        End = (Byte: 15, Line: 1, Column: 16),
					        },
				        },
			        },
		        },

		        // Heredoc Templates
		        new TestCase
		        {
			        InputString = @"<<EOT
hello world
EOT
",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOHeredoc,
					        ExpectedBytes = Encoding.UTF8.GetBytes("<<EOT\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 6, Line: 2, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenStringLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("hello world\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 6, Line: 2, Column: 1),
						        End = (Byte: 18, Line: 3, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCHeredoc,
					        ExpectedBytes = Encoding.UTF8.GetBytes("EOT"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 18, Line: 3, Column: 1),
						        End = (Byte: 21, Line: 3, Column: 4),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNewline,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 21, Line: 3, Column: 4),
						        End = (Byte: 22, Line: 4, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 22, Line: 4, Column: 1),
						        End = (Byte: 22, Line: 4, Column: 1),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = "<<EOT\r\nhello world\r\nEOT\r\n", // intentional windows-style line endings
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOHeredoc,
					        ExpectedBytes = Encoding.UTF8.GetBytes("<<EOT\r\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 7, Line: 2, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenStringLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("hello world\r\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 7, Line: 2, Column: 1),
						        End = (Byte: 20, Line: 3, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCHeredoc,
					        ExpectedBytes = Encoding.UTF8.GetBytes("EOT"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 20, Line: 3, Column: 1),
						        End = (Byte: 23, Line: 3, Column: 4),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNewline,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\r\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 23, Line: 3, Column: 4),
						        End = (Byte: 25, Line: 4, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 25, Line: 4, Column: 1),
						        End = (Byte: 25, Line: 4, Column: 1),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = @"<<EOT
hello ${name}
EOT
",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOHeredoc,
					        ExpectedBytes = Encoding.UTF8.GetBytes("<<EOT\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 6, Line: 2, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenStringLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("hello "),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 6, Line: 2, Column: 1),
						        End = (Byte: 12, Line: 2, Column: 7),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenTemplateInterp,
					        ExpectedBytes = Encoding.UTF8.GetBytes("${"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 12, Line: 2, Column: 7),
						        End = (Byte: 14, Line: 2, Column: 9),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenIdent,
					        ExpectedBytes = Encoding.UTF8.GetBytes("name"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 14, Line: 2, Column: 9),
						        End = (Byte: 18, Line: 2, Column: 13),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenTemplateSeqEnd,
					        ExpectedBytes = Encoding.UTF8.GetBytes("}"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 18, Line: 2, Column: 13),
						        End = (Byte: 19, Line: 2, Column: 14),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenStringLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 19, Line: 2, Column: 14),
						        End = (Byte: 20, Line: 3, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCHeredoc,
					        ExpectedBytes = Encoding.UTF8.GetBytes("EOT"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 20, Line: 3, Column: 1),
						        End = (Byte: 23, Line: 3, Column: 4),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNewline,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 23, Line: 3, Column: 4),
						        End = (Byte: 24, Line: 4, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 24, Line: 4, Column: 1),
						        End = (Byte: 24, Line: 4, Column: 1),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = @"<<EOT
${name}EOT
EOT
",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOHeredoc,
					        ExpectedBytes = Encoding.UTF8.GetBytes("<<EOT\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 6, Line: 2, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenTemplateInterp,
					        ExpectedBytes = Encoding.UTF8.GetBytes("${"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 6, Line: 2, Column: 1),
						        End = (Byte: 8, Line: 2, Column: 3),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenIdent,
					        ExpectedBytes = Encoding.UTF8.GetBytes("name"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 8, Line: 2, Column: 3),
						        End = (Byte: 12, Line: 2, Column: 7),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenTemplateSeqEnd,
					        ExpectedBytes = Encoding.UTF8.GetBytes("}"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 12, Line: 2, Column: 7),
						        End = (Byte: 13, Line: 2, Column: 8),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenStringLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("EOT\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 13, Line: 2, Column: 8),
						        End = (Byte: 17, Line: 3, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCHeredoc,
					        ExpectedBytes = Encoding.UTF8.GetBytes("EOT"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 17, Line: 3, Column: 1),
						        End = (Byte: 20, Line: 3, Column: 4),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNewline,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 20, Line: 3, Column: 4),
						        End = (Byte: 21, Line: 4, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 21, Line: 4, Column: 1),
						        End = (Byte: 21, Line: 4, Column: 1),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = @"<<EOF
${<<-EOF
hello
EOF
}
EOF
",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOHeredoc,
					        ExpectedBytes = Encoding.UTF8.GetBytes("<<EOF\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 6, Line: 2, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenTemplateInterp,
					        ExpectedBytes = Encoding.UTF8.GetBytes("${"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 6, Line: 2, Column: 1),
						        End = (Byte: 8, Line: 2, Column: 3),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOHeredoc,
					        ExpectedBytes = Encoding.UTF8.GetBytes("<<-EOF\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 8, Line: 2, Column: 3),
						        End = (Byte: 15, Line: 3, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenStringLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("hello\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 15, Line: 3, Column: 1),
						        End = (Byte: 21, Line: 4, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCHeredoc,
					        ExpectedBytes = Encoding.UTF8.GetBytes("EOF"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 21, Line: 4, Column: 1),
						        End = (Byte: 24, Line: 4, Column: 4),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNewline,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 24, Line: 4, Column: 4),
						        End = (Byte: 25, Line: 5, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenTemplateSeqEnd,
					        ExpectedBytes = Encoding.UTF8.GetBytes("}"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 25, Line: 5, Column: 1),
						        End = (Byte: 26, Line: 5, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenStringLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 26, Line: 5, Column: 2),
						        End = (Byte: 27, Line: 6, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCHeredoc,
					        ExpectedBytes = Encoding.UTF8.GetBytes("EOF"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 27, Line: 6, Column: 1),
						        End = (Byte: 30, Line: 6, Column: 4),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNewline,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 30, Line: 6, Column: 4),
						        End = (Byte: 31, Line: 7, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 31, Line: 7, Column: 1),
						        End = (Byte: 31, Line: 7, Column: 1),
					        },
				        },
			        },
		        },

		        // Combinations
		        new TestCase
		        {
			        InputString = @" (1 + 2) * 3",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOParen,
					        ExpectedBytes = Encoding.UTF8.GetBytes("("),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 2, Line: 1, Column: 3),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNumberLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("1"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 2, Line: 1, Column: 3),
						        End = (Byte: 3, Line: 1, Column: 4),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenPlus,
					        ExpectedBytes = Encoding.UTF8.GetBytes("+"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 4, Line: 1, Column: 5),
						        End = (Byte: 5, Line: 1, Column: 6),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNumberLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("2"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 6, Line: 1, Column: 7),
						        End = (Byte: 7, Line: 1, Column: 8),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCParen,
					        ExpectedBytes = Encoding.UTF8.GetBytes(")"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 7, Line: 1, Column: 8),
						        End = (Byte: 8, Line: 1, Column: 9),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenStar,
					        ExpectedBytes = Encoding.UTF8.GetBytes("*"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 9, Line: 1, Column: 10),
						        End = (Byte: 10, Line: 1, Column: 11),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNumberLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("3"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 11, Line: 1, Column: 12),
						        End = (Byte: 12, Line: 1, Column: 13),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 13, Line: 1, Column: 14),
						        End = (Byte: 13, Line: 1, Column: 14),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = @"9%8",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNumberLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("9"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenPercent,
					        ExpectedBytes = Encoding.UTF8.GetBytes("%"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 2, Line: 1, Column: 3),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNumberLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("8"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 2, Line: 1, Column: 3),
						        End = (Byte: 3, Line: 1, Column: 4),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = Encoding.UTF8.GetBytes(""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 3, Line: 1, Column: 4),
						        End = (Byte: 3, Line: 1, Column: 4),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = "\na = 1\n",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNewline,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 2, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenIdent,
					        ExpectedBytes = Encoding.UTF8.GetBytes("a"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 2, Column: 1),
						        End = (Byte: 2, Line: 2, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEqual,
					        ExpectedBytes = Encoding.UTF8.GetBytes("="),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 3, Line: 2, Column: 3),
						        End = (Byte: 4, Line: 2, Column: 4),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNumberLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("1"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 5, Line: 2, Column: 5),
						        End = (Byte: 6, Line: 2, Column: 6),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNewline,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 6, Line: 2, Column: 6),
						        End = (Byte: 7, Line: 3, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 7, Line: 3, Column: 1),
						        End = (Byte: 7, Line: 3, Column: 1),
					        },
				        },
			        },
		        },

		        // Comments
		        new TestCase
		        {
			        InputString = "# hello\n",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenComment,
					        ExpectedBytes = Encoding.UTF8.GetBytes("# hello\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 8, Line: 2, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 8, Line: 2, Column: 1),
						        End = (Byte: 8, Line: 2, Column: 1),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = "// hello\n",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenComment,
					        ExpectedBytes = Encoding.UTF8.GetBytes("// hello\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 9, Line: 2, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 9, Line: 2, Column: 1),
						        End = (Byte: 9, Line: 2, Column: 1),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = "// hello\n// hello",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenComment,
					        ExpectedBytes = Encoding.UTF8.GetBytes("// hello\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 9, Line: 2, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenComment,
					        ExpectedBytes = Encoding.UTF8.GetBytes("// hello"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 9, Line: 2, Column: 1),
						        End = (Byte: 17, Line: 2, Column: 9),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 17, Line: 2, Column: 9),
						        End = (Byte: 17, Line: 2, Column: 9),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = "// hello\nfoo\n// hello",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenComment,
					        ExpectedBytes = Encoding.UTF8.GetBytes("// hello\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 9, Line: 2, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenIdent,
					        ExpectedBytes = Encoding.UTF8.GetBytes("foo"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 9, Line: 2, Column: 1),
						        End = (Byte: 12, Line: 2, Column: 4),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNewline,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 12, Line: 2, Column: 4),
						        End = (Byte: 13, Line: 3, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenComment,
					        ExpectedBytes = Encoding.UTF8.GetBytes("// hello"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 13, Line: 3, Column: 1),
						        End = (Byte: 21, Line: 3, Column: 9),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 21, Line: 3, Column: 9),
						        End = (Byte: 21, Line: 3, Column: 9),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = "# hello\n# hello",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenComment,
					        ExpectedBytes = Encoding.UTF8.GetBytes("# hello\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 8, Line: 2, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenComment,
					        ExpectedBytes = Encoding.UTF8.GetBytes("# hello"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 8, Line: 2, Column: 1),
						        End = (Byte: 15, Line: 2, Column: 8),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 15, Line: 2, Column: 8),
						        End = (Byte: 15, Line: 2, Column: 8),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = "# hello\nfoo\n# hello",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenComment,
					        ExpectedBytes = Encoding.UTF8.GetBytes("# hello\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 8, Line: 2, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenIdent,
					        ExpectedBytes = Encoding.UTF8.GetBytes("foo"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 8, Line: 2, Column: 1),
						        End = (Byte: 11, Line: 2, Column: 4),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNewline,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\n"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 11, Line: 2, Column: 4),
						        End = (Byte: 12, Line: 3, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenComment,
					        ExpectedBytes = Encoding.UTF8.GetBytes("# hello"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 12, Line: 3, Column: 1),
						        End = (Byte: 19, Line: 3, Column: 8),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 19, Line: 3, Column: 8),
						        End = (Byte: 19, Line: 3, Column: 8),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = @"/* hello */",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenComment,
					        ExpectedBytes = Encoding.UTF8.GetBytes("/* hello */"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 11, Line: 1, Column: 12),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 11, Line: 1, Column: 12),
						        End = (Byte: 11, Line: 1, Column: 12),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = @"/* hello */ howdy /* hey */",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenComment,
					        ExpectedBytes = Encoding.UTF8.GetBytes("/* hello */"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 11, Line: 1, Column: 12),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenIdent,
					        ExpectedBytes = Encoding.UTF8.GetBytes("howdy"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 12, Line: 1, Column: 13),
						        End = (Byte: 17, Line: 1, Column: 18),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenComment,
					        ExpectedBytes = Encoding.UTF8.GetBytes("/* hey */"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 18, Line: 1, Column: 19),
						        End = (Byte: 27, Line: 1, Column: 28),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 27, Line: 1, Column: 28),
						        End = (Byte: 27, Line: 1, Column: 28),
					        },
				        },
			        },
		        },

		        // Invalid things
		        new TestCase
		        {
			        Input = Encoding.UTF8.GetBytes("🌻"),
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenInvalid,
					        ExpectedBytes = Encoding.UTF8.GetBytes("🌻"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 4, Line: 1, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 4, Line: 1, Column: 2),
						        End = (Byte: 4, Line: 1, Column: 2),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = @"|",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenBitwiseOr,
					        ExpectedBytes = Encoding.UTF8.GetBytes("|"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        Input = new byte[] { 0x80 }, // UTF-8 continuation without an introducer
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenBadUTF8,
					        ExpectedBytes = new byte[] {0x80},
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 1, Line: 1, Column: 2),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        Input = new byte[] { 0x20, 0x80, 0x80 }, // UTF-8 continuation without an introducer
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenBadUTF8,
					        ExpectedBytes = new byte[] {0x80},
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 1, Line: 1, Column: 2),
						        End = (Byte: 2, Line: 1, Column: 3),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenBadUTF8,
					        ExpectedBytes = new byte[] {0x80},
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 2, Line: 1, Column: 3),
						        End = (Byte: 3, Line: 1, Column: 4),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 3, Line: 1, Column: 4),
						        End = (Byte: 3, Line: 1, Column: 4),
					        },
				        },
			        },
		        },
		        new TestCase
		        {
			        InputString = "\t\t",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 2, Line: 1, Column: 3),
						        End = (Byte: 2, Line: 1, Column: 3),
					        },
				        },
			        },
		        },

		        // Misc combinations that have come up in bug reports, etc.
		        new TestCase
		        {
			        InputString = "locals {\n  is_percent = percent_sign == \"%\" ? true : false\n}\n",
			        ExpectedTokens = new[]
			        {
				        new ExpectedToken
				        {
					        Type = TokenType.TokenIdent,
					        ExpectedBytes = Encoding.UTF8.GetBytes("locals"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 0, Line: 1, Column: 1),
						        End = (Byte: 6, Line: 1, Column: 7),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOBrace,
					        ExpectedBytes = new byte[] {(byte)'{'},
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 7, Line: 1, Column: 8),
						        End = (Byte: 8, Line: 1, Column: 9),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNewline,
					        ExpectedBytes = new byte[] {(byte)'\n'},
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 8, Line: 1, Column: 9),
						        End = (Byte: 9, Line: 2, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenIdent,
					        ExpectedBytes = Encoding.UTF8.GetBytes("is_percent"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 11, Line: 2, Column: 3),
						        End = (Byte: 21, Line: 2, Column: 13),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEqual,
					        ExpectedBytes = Encoding.UTF8.GetBytes("="),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 22, Line: 2, Column: 14),
						        End = (Byte: 23, Line: 2, Column: 15),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenIdent,
					        ExpectedBytes = Encoding.UTF8.GetBytes("percent_sign"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 24, Line: 2, Column: 16),
						        End = (Byte: 36, Line: 2, Column: 28),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEqualOp,
					        ExpectedBytes = Encoding.UTF8.GetBytes("=="),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 37, Line: 2, Column: 29),
						        End = (Byte: 39, Line: 2, Column: 31),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenOQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 40, Line: 2, Column: 32),
						        End = (Byte: 41, Line: 2, Column: 33),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuotedLit,
					        ExpectedBytes = Encoding.UTF8.GetBytes("%"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 41, Line: 2, Column: 33),
						        End = (Byte: 42, Line: 2, Column: 34),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCQuote,
					        ExpectedBytes = Encoding.UTF8.GetBytes("\""),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 42, Line: 2, Column: 34),
						        End = (Byte: 43, Line: 2, Column: 35),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenQuestion,
					        ExpectedBytes = Encoding.UTF8.GetBytes("?"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 44, Line: 2, Column: 36),
						        End = (Byte: 45, Line: 2, Column: 37),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenIdent,
					        ExpectedBytes = Encoding.UTF8.GetBytes("true"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 46, Line: 2, Column: 38),
						        End = (Byte: 50, Line: 2, Column: 42),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenColon,
					        ExpectedBytes = Encoding.UTF8.GetBytes(":"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 51, Line: 2, Column: 43),
						        End = (Byte: 52, Line: 2, Column: 44),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenIdent,
					        ExpectedBytes = Encoding.UTF8.GetBytes("false"),
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 53, Line: 2, Column: 45),
						        End = (Byte: 58, Line: 2, Column: 50),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNewline,
					        ExpectedBytes = new byte[] {(byte)'\n'},
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 58, Line: 2, Column: 50),
						        End = (Byte: 59, Line: 3, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenCBrace,
					        ExpectedBytes = new byte[] {(byte)'}'},
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 59, Line: 3, Column: 1),
						        End = (Byte: 60, Line: 3, Column: 2),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenNewline,
					        ExpectedBytes = new byte[] {(byte)'\n'},
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 60, Line: 3, Column: 2),
						        End = (Byte: 61, Line: 4, Column: 1),
					        },
				        },
				        new ExpectedToken
				        {
					        Type = TokenType.TokenEOF,
					        ExpectedBytes = new byte[] { },
					        ExpectedRange = new ExpectedRange
					        {
						        Start = (Byte: 61, Line: 4, Column: 1),
						        End = (Byte: 61, Line: 4, Column: 1),
					        },
				        },
			        },
		        }
	        }
		        .Select(ToTestCaseData)
		        .ToArray();
        }

        private static TestCaseData ToTestCaseData(TestCase tc, int i)
        {
	        string input = tc.InputString ?? string.Join("", tc.Input.Select(b => $"\\x{b.ToString("X")}"));
	        return new TestCaseData(tc).SetName($"{i:D2}: Input: {input}");
        }

        private static TestCaseData[] TemplateTestCases()
        {
	        return new[]
{
		// Empty input
		new TestCase
				{
					InputString = "",
					ExpectedTokens = new []
					{
				new ExpectedToken
						{
							Type = TokenType.TokenEOF,
							ExpectedBytes = new byte[] {},
							ExpectedRange = new ExpectedRange
							{
								Start = ( Byte: 0, Line: 1, Column: 1 ),
								End = ( Byte: 0, Line: 1, Column: 1 ),
							},
						},
			},
		},

		// Simple literals
		new TestCase
				{
					InputString = " hello ",
					ExpectedTokens = new []
					{
				new ExpectedToken
						{
							Type = TokenType.TokenStringLit,
							ExpectedBytes = Encoding.UTF8.GetBytes(" hello "),
							ExpectedRange = new ExpectedRange
							{
								Start = ( Byte: 0, Line: 1, Column: 1 ),
								End = ( Byte: 7, Line: 1, Column: 8 ),
							},
						},
				new ExpectedToken
						{
							Type = TokenType.TokenEOF,
							ExpectedBytes = new byte[] {},
							ExpectedRange = new ExpectedRange
							{
								Start = ( Byte: 7, Line: 1, Column: 8 ),
								End = ( Byte: 7, Line: 1, Column: 8 ),
							},
						},
			},
		},
		new TestCase
				{
					InputString = "\nhello\n",
					ExpectedTokens = new []
					{
				new ExpectedToken
						{
							Type = TokenType.TokenStringLit,
							ExpectedBytes = Encoding.UTF8.GetBytes("\n"),
							ExpectedRange = new ExpectedRange
							{
								Start = ( Byte: 0, Line: 1, Column: 1 ),
								End = ( Byte: 1, Line: 2, Column: 1 ),
							},
						},
				new ExpectedToken
						{
							Type = TokenType.TokenStringLit,
							ExpectedBytes = Encoding.UTF8.GetBytes("hello\n"),
							ExpectedRange = new ExpectedRange
							{
								Start = ( Byte: 1, Line: 2, Column: 1 ),
								End = ( Byte: 7, Line: 3, Column: 1 ),
							},
						},
				new ExpectedToken
						{
							Type = TokenType.TokenEOF,
							ExpectedBytes = new byte[] {},
							ExpectedRange = new ExpectedRange
							{
								Start = ( Byte: 7, Line: 3, Column: 1 ),
								End = ( Byte: 7, Line: 3, Column: 1 ),
							},
						},
			},
		},
		new TestCase
				{
					InputString = "hello ${foo} hello",
					ExpectedTokens = new []
					{
				new ExpectedToken
						{
							Type = TokenType.TokenStringLit,
							ExpectedBytes = Encoding.UTF8.GetBytes("hello "),
							ExpectedRange = new ExpectedRange
							{
								Start = ( Byte: 0, Line: 1, Column: 1 ),
								End = ( Byte: 6, Line: 1, Column: 7 ),
							},
						},
				new ExpectedToken
						{
							Type = TokenType.TokenTemplateInterp,
							ExpectedBytes = Encoding.UTF8.GetBytes("${"),
							ExpectedRange = new ExpectedRange
							{
								Start = ( Byte: 6, Line: 1, Column: 7 ),
								End = ( Byte: 8, Line: 1, Column: 9 ),
							},
						},
				new ExpectedToken
						{
							Type = TokenType.TokenIdent,
							ExpectedBytes = Encoding.UTF8.GetBytes("foo"),
							ExpectedRange = new ExpectedRange
							{
								Start = ( Byte: 8, Line: 1, Column: 9 ),
								End = ( Byte: 11, Line: 1, Column: 12 ),
							},
						},
				new ExpectedToken
						{
							Type = TokenType.TokenTemplateSeqEnd,
							ExpectedBytes = Encoding.UTF8.GetBytes("}"),
							ExpectedRange = new ExpectedRange
							{
								Start = ( Byte: 11, Line: 1, Column: 12 ),
								End = ( Byte: 12, Line: 1, Column: 13 ),
							},
						},
				new ExpectedToken
						{
							Type = TokenType.TokenStringLit,
							ExpectedBytes = Encoding.UTF8.GetBytes(" hello"),
							ExpectedRange = new ExpectedRange
							{
								Start = ( Byte: 12, Line: 1, Column: 13 ),
								End = ( Byte: 18, Line: 1, Column: 19 ),
							},
						},
				new ExpectedToken
						{
							Type = TokenType.TokenEOF,
							ExpectedBytes = new byte[] {},
							ExpectedRange = new ExpectedRange
							{
								Start = ( Byte: 18, Line: 1, Column: 19 ),
								End = ( Byte: 18, Line: 1, Column: 19 ),
							},
						},
			},
		},
		new TestCase
				{
					InputString = "hello ${~foo~} hello",
					ExpectedTokens = new []
					{
				new ExpectedToken
						{
							Type = TokenType.TokenStringLit,
							ExpectedBytes = Encoding.UTF8.GetBytes("hello "),
							ExpectedRange = new ExpectedRange
							{
								Start = ( Byte: 0, Line: 1, Column: 1 ),
								End = ( Byte: 6, Line: 1, Column: 7 ),
							},
						},
				new ExpectedToken
						{
							Type = TokenType.TokenTemplateInterp,
							ExpectedBytes = Encoding.UTF8.GetBytes("${~"),
							ExpectedRange = new ExpectedRange
							{
								Start = ( Byte: 6, Line: 1, Column: 7 ),
								End = ( Byte: 9, Line: 1, Column: 10 ),
							},
						},
				new ExpectedToken
						{
							Type = TokenType.TokenIdent,
							ExpectedBytes = Encoding.UTF8.GetBytes("foo"),
							ExpectedRange = new ExpectedRange
							{
								Start = ( Byte: 9, Line: 1, Column: 10 ),
								End = ( Byte: 12, Line: 1, Column: 13 ),
							},
						},
				new ExpectedToken
						{
							Type = TokenType.TokenTemplateSeqEnd,
							ExpectedBytes = Encoding.UTF8.GetBytes("~}"),
							ExpectedRange = new ExpectedRange
							{
								Start = ( Byte: 12, Line: 1, Column: 13 ),
								End = ( Byte: 14, Line: 1, Column: 15 ),
							},
						},
				new ExpectedToken
						{
							Type = TokenType.TokenStringLit,
							ExpectedBytes = Encoding.UTF8.GetBytes(" hello"),
							ExpectedRange = new ExpectedRange
							{
								Start = ( Byte: 14, Line: 1, Column: 15 ),
								End = ( Byte: 20, Line: 1, Column: 21 ),
							},
						},
				new ExpectedToken
						{
							Type = TokenType.TokenEOF,
							ExpectedBytes = new byte[] {},
							ExpectedRange = new ExpectedRange
							{
								Start = ( Byte: 20, Line: 1, Column: 21 ),
								End = ( Byte: 20, Line: 1, Column: 21 ),
							},
						},
			},
		},
            }
		        .Select(ToTestCaseData)
		        .ToArray();;
        }

        public class TestCase
        {
	        public string InputString { get; set; }
            public byte[] Input { get; set; }
            public ExpectedToken[] ExpectedTokens { get; set; }
        }

        public class ExpectedToken
        {
            public TokenType Type { get; set; }
            public byte[] ExpectedBytes { get; set; }
            public ExpectedRange ExpectedRange { get; set; }
        }

        public class ExpectedRange
        {
            public (int Byte, int Line, int Column) Start { get; set; }
            public (int Byte, int Line, int Column) End { get; set; }
        }
    }
}