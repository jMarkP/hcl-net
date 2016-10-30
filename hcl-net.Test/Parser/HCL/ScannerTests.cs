using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using hcl_net.Parser.HCL;
using NUnit.Framework;

namespace hcl_net.Test.Parser.HCL
{
    [TestFixture]
    class ScannerTests
    {
        [Test]
        public void TestPosition()
        {
            var testDocument = string.Join("",
                tokenLists
                    .SelectMany(x => x.Value)
                    .Select(v => string.Format("\t\t\t\t{0}\n", v.Item2)));

            var SUT = new Scanner(testDocument, "TestDocument.hcl");
            var expectedPos = Pos.CreateForFile("TestDocument.hcl");
            char ch;
            foreach (var tokenList in tokenLists)
            {
                foreach (var entry in tokenList.Value)
                {
                    // First advance the expectedPos by 4 tabs
                    for (var i = 0; i < 4; i++)
                    {
                        expectedPos = expectedPos.NextInString(testDocument, out ch);
                    }
                    // Make the scanner scan another token
                    var actual = SUT.Scan();
                    Assert.That(actual.Pos.Offset, Is.EqualTo(expectedPos.Offset), "Wrong offset for {0}:{1}", tokenList.Key, entry.Item2);
                    Assert.That(actual.Pos.Line, Is.EqualTo(expectedPos.Line), "Wrong line for {0}:{1}", tokenList.Key, entry.Item2);
                    Assert.That(actual.Pos.Column, Is.EqualTo(expectedPos.Column), "Wrong column for {0}:{1}", tokenList.Key, entry.Item2);
                    Assert.That(actual.Pos.Filename, Is.EqualTo(expectedPos.Filename), "Wrong filename for {0}:{1}", tokenList.Key, entry.Item2);
                    Assert.That(SUT.ErrorCount, Is.EqualTo(0), "Should have no errors");

                    // Advance the ExpectedPos for the read in string
                    foreach (var _ in actual.Text)
                    {
                        expectedPos = expectedPos.NextInString(testDocument, out ch);
                    }
                    // Then advance the expectedPos by 1 more for the newline
                    expectedPos = expectedPos.NextInString(testDocument, out ch);
                }
            }
        }

        [TestCaseSource("TestTypes")]
        public void TestTokenList(string tokenList)
        {
            var list = tokenLists[tokenList];
            var doc = string.Join("",
                list.Select(s => s.Item2 + "\n"));
            var SUT = new Scanner(doc);
            foreach (var expected in list)
            {
                Console.WriteLine("Testing: " + expected.Item2);
                var actual = SUT.Scan();
                Assert.That(actual, Is.Not.Null);
                Assert.That(actual.Text, Is.EqualTo(expected.Item2));
                Assert.That(actual.Type, Is.EqualTo(expected.Item1));
                Assert.That(SUT.ErrorCount, Is.EqualTo(0), "Should have no errors");
            }
        }

        [Test]
        public void TestNullChar()
        {
            var SUT = new Scanner(@"""\0");
            // Shouldn't throw
            SUT.Scan();
        }

        [Test]
        public void TestWindowsLineEndings()
        {
            var text =
                "// This should have Windows line endings\r\nresource \"aws_instance\" \"foo\" {\r\n    user_data=<<HEREDOC\r\n    test script\r\nHEREDOC\r\n}";
            var expected = new[]
            {
                Tuple.Create(TokenType.COMMENT, "// This should have Windows line endings\r"),
                Tuple.Create(TokenType.IDENT, "resource"),
                Tuple.Create(TokenType.STRING, "\"aws_instance\""),
                Tuple.Create(TokenType.STRING, "\"foo\""),
                Tuple.Create(TokenType.LBRACE, "{"),
                Tuple.Create(TokenType.IDENT, "user_data"),
                Tuple.Create(TokenType.ASSIGN, "="),
                Tuple.Create(TokenType.HEREDOC, "<<HEREDOC\r\n    test script\r\nHEREDOC\r"),
                Tuple.Create(TokenType.RBRACE, "}")
            };

            var SUT = new Scanner(text);
            foreach (var entry in expected)
            {
                var actual = SUT.Scan();
                Assert.That(actual.Text, Is.EqualTo(entry.Item2));
                Assert.That(actual.Type, Is.EqualTo(entry.Item1));
                Assert.That(SUT.ErrorCount, Is.EqualTo(0));
            }
        }

        [Test]
        public void TestRealExample()
        {
            var text = @"// This comes from Terraform, as a test
	variable ""foo"" {
        default = ""bar""
        description = ""bar""
    }

    provider ""aws"" {
	  access_key = ""foo""
	  secret_key = ""bar""
	}

    resource ""aws_security_group"" ""firewall"" {
	    count = 5
	}

    resource aws_instance ""web"" {
	    ami = ""${var.foo}""
	    security_groups = [
	        ""foo"",
	        ""${aws_security_group.firewall.foo}""
	    ]

        network_interface {
	        device_index = 0
	        description = <<EOF
Main interface
EOF
	    }

		network_interface {
	        device_index = 1
	        description = <<-EOF
            Outer text
                Indented text
            EOF
		}
	}".Replace("\r\n", "\n").Replace("    ", "\t");
            var expectedValues = new[]
            {
                Tuple.Create(TokenType.COMMENT, @"// This comes from Terraform, as a test"),
                Tuple.Create(TokenType.IDENT, @"variable"),
                Tuple.Create(TokenType.STRING, @"""foo"""),
                Tuple.Create(TokenType.LBRACE, @"{"),
                Tuple.Create(TokenType.IDENT, @"default"),
                Tuple.Create(TokenType.ASSIGN, @"="),
                Tuple.Create(TokenType.STRING, @"""bar"""),
                Tuple.Create(TokenType.IDENT, @"description"),
                Tuple.Create(TokenType.ASSIGN, @"="),
                Tuple.Create(TokenType.STRING, @"""bar"""),
                Tuple.Create(TokenType.RBRACE, @"}"),
                Tuple.Create(TokenType.IDENT, @"provider"),
                Tuple.Create(TokenType.STRING, @"""aws"""),
                Tuple.Create(TokenType.LBRACE, @"{"),
                Tuple.Create(TokenType.IDENT, @"access_key"),
                Tuple.Create(TokenType.ASSIGN, @"="),
                Tuple.Create(TokenType.STRING, @"""foo"""),
                Tuple.Create(TokenType.IDENT, @"secret_key"),
                Tuple.Create(TokenType.ASSIGN, @"="),
                Tuple.Create(TokenType.STRING, @"""bar"""),
                Tuple.Create(TokenType.RBRACE, @"}"),
                Tuple.Create(TokenType.IDENT, @"resource"),
                Tuple.Create(TokenType.STRING, @"""aws_security_group"""),
                Tuple.Create(TokenType.STRING, @"""firewall"""),
                Tuple.Create(TokenType.LBRACE, @"{"),
                Tuple.Create(TokenType.IDENT, @"count"),
                Tuple.Create(TokenType.ASSIGN, @"="),
                Tuple.Create(TokenType.NUMBER, @"5"),
                Tuple.Create(TokenType.RBRACE, @"}"),
                Tuple.Create(TokenType.IDENT, @"resource"),
                Tuple.Create(TokenType.IDENT, @"aws_instance"),
                Tuple.Create(TokenType.STRING, @"""web"""),
                Tuple.Create(TokenType.LBRACE, @"{"),
                Tuple.Create(TokenType.IDENT, @"ami"),
                Tuple.Create(TokenType.ASSIGN, @"="),
                Tuple.Create(TokenType.STRING, @"""${var.foo}"""),
                Tuple.Create(TokenType.IDENT, @"security_groups"),
                Tuple.Create(TokenType.ASSIGN, @"="),
                Tuple.Create(TokenType.LBRACK, @"["),
                Tuple.Create(TokenType.STRING, @"""foo"""),
                Tuple.Create(TokenType.COMMA, @","),
                Tuple.Create(TokenType.STRING, @"""${aws_security_group.firewall.foo}"""),
                Tuple.Create(TokenType.RBRACK, @"]"),
                Tuple.Create(TokenType.IDENT, @"network_interface"),
                Tuple.Create(TokenType.LBRACE, @"{"),
                Tuple.Create(TokenType.IDENT, @"device_index"),
                Tuple.Create(TokenType.ASSIGN, @"="),
                Tuple.Create(TokenType.NUMBER, @"0"),
                Tuple.Create(TokenType.IDENT, @"description"),
                Tuple.Create(TokenType.ASSIGN, @"="),
                Tuple.Create(TokenType.HEREDOC, "<<EOF\nMain interface\nEOF"),
                Tuple.Create(TokenType.RBRACE, @"}"),
                Tuple.Create(TokenType.IDENT, @"network_interface"),
                Tuple.Create(TokenType.LBRACE, @"{"),
                Tuple.Create(TokenType.IDENT, @"device_index"),
                Tuple.Create(TokenType.ASSIGN, @"="),
                Tuple.Create(TokenType.NUMBER, @"1"),
                Tuple.Create(TokenType.IDENT, @"description"),
                Tuple.Create(TokenType.ASSIGN, @"="),
                Tuple.Create(TokenType.HEREDOC, "<<-EOF\n\t\t\tOuter text\n\t\t\t\tIndented text\n\t\t\tEOF"),
                Tuple.Create(TokenType.RBRACE, @"}"),
                Tuple.Create(TokenType.RBRACE, @"}"),
                Tuple.Create(TokenType.EOF, @""),
            };

            var SUT = new Scanner(text);
            foreach (var expected in expectedValues)
            {
                var actual = SUT.Scan();
                Assert.That(actual.Text, Is.EqualTo(expected.Item2));
                Assert.That(actual.Type, Is.EqualTo(expected.Item1));
                Assert.That(SUT.ErrorCount, Is.EqualTo(0));
            }
        }

        #region Test data

        private static string[] TestTypes()
        {
            return tokenLists.Keys.ToArray();
        }

        private static readonly string F100 = string.Join("", Enumerable.Repeat("f", 100));
        private static Dictionary<string, Tuple<TokenType, string>[]> tokenLists = new Dictionary<string, Tuple<TokenType, string>[]>
    { { "comment", new [] {
		Tuple.Create(TokenType.COMMENT, "//"),
		Tuple.Create(TokenType.COMMENT, "////"),
		Tuple.Create(TokenType.COMMENT, "// comment"),
		Tuple.Create(TokenType.COMMENT, "// /* comment */"),
		Tuple.Create(TokenType.COMMENT, "// // comment //"),
		Tuple.Create(TokenType.COMMENT, "//" + F100),
		Tuple.Create(TokenType.COMMENT, "#"),
		Tuple.Create(TokenType.COMMENT, "##"),
		Tuple.Create(TokenType.COMMENT, "# comment"),
		Tuple.Create(TokenType.COMMENT, "# /* comment */"),
		Tuple.Create(TokenType.COMMENT, "# # comment #"),
		Tuple.Create(TokenType.COMMENT, "#" + F100),
		Tuple.Create(TokenType.COMMENT, "/**/"),
		Tuple.Create(TokenType.COMMENT, "/***/"),
		Tuple.Create(TokenType.COMMENT, "/* comment */"),
		Tuple.Create(TokenType.COMMENT, "/* // comment */"),
		Tuple.Create(TokenType.COMMENT, "/* /* comment */"),
		Tuple.Create(TokenType.COMMENT, "/*\n comment\n*/"),
		Tuple.Create(TokenType.COMMENT, "/*" + F100 + "*/"),
	}},
	{ "operator", new [] {
		Tuple.Create(TokenType.LBRACK, "["),
		Tuple.Create(TokenType.LBRACE, "{"),
		Tuple.Create(TokenType.COMMA, ","),
		Tuple.Create(TokenType.PERIOD, "."),
		Tuple.Create(TokenType.RBRACK, "]"),
		Tuple.Create(TokenType.RBRACE, "}"),
		Tuple.Create(TokenType.ASSIGN, "="),
		Tuple.Create(TokenType.ADD, "+"),
		Tuple.Create(TokenType.SUB, "-"),
	}},
    { "bool", new []{
		Tuple.Create(TokenType.BOOL, "true"),
		Tuple.Create(TokenType.BOOL, "false"),
	}},
    { "ident", new []{
		Tuple.Create(TokenType.IDENT, "a"),
		Tuple.Create(TokenType.IDENT, "a0"),
		Tuple.Create(TokenType.IDENT, "foobar"),
		Tuple.Create(TokenType.IDENT, "foo-bar"),
		Tuple.Create(TokenType.IDENT, "abc123"),
		Tuple.Create(TokenType.IDENT, "LGTM"),
		Tuple.Create(TokenType.IDENT, "_"),
		Tuple.Create(TokenType.IDENT, "_abc123"),
		Tuple.Create(TokenType.IDENT, "abc123_"),
		Tuple.Create(TokenType.IDENT, "_abc_123_"),
		Tuple.Create(TokenType.IDENT, "_äöü"),
		Tuple.Create(TokenType.IDENT, "_本"),
		Tuple.Create(TokenType.IDENT, "äöü"),
		Tuple.Create(TokenType.IDENT, "本"),
		Tuple.Create(TokenType.IDENT, "a۰۱۸"),
		Tuple.Create(TokenType.IDENT, "foo६४"),
		Tuple.Create(TokenType.IDENT, "bar９８７６"),
	}},
    { "heredoc", new []{
		Tuple.Create(TokenType.HEREDOC, "<<EOF\nhello\nworld\nEOF"),
		Tuple.Create(TokenType.HEREDOC, "<<EOF123\nhello\nworld\nEOF123"),
        Tuple.Create(TokenType.HEREDOC, "<<-EOF123\n\thello\n\tworld\n\tEOF123"),
    }},
    { "string", new []{
		Tuple.Create(TokenType.STRING, @""" """),
		Tuple.Create(TokenType.STRING, @"""a"""),
		Tuple.Create(TokenType.STRING, @"""本"""),
		Tuple.Create(TokenType.STRING, @"""${file(""foo"")}"""),
		Tuple.Create(TokenType.STRING, @"""${file(\""foo\"")}"""),
		Tuple.Create(TokenType.STRING, @"""\a"""),
		Tuple.Create(TokenType.STRING, @"""\b"""),
		Tuple.Create(TokenType.STRING, @"""\f"""),
		Tuple.Create(TokenType.STRING, @"""\n"""),
		Tuple.Create(TokenType.STRING, @"""\r"""),
		Tuple.Create(TokenType.STRING, @"""\t"""),
		Tuple.Create(TokenType.STRING, @"""\v"""),
		Tuple.Create(TokenType.STRING, @"""\"""""),
		Tuple.Create(TokenType.STRING, @"""\000"""),
		Tuple.Create(TokenType.STRING, @"""\777"""),
		Tuple.Create(TokenType.STRING, @"""\x00"""),
		Tuple.Create(TokenType.STRING, @"""\xff"""),
		Tuple.Create(TokenType.STRING, @"""\u0000"""),
		Tuple.Create(TokenType.STRING, @"""\ufA16"""),
		Tuple.Create(TokenType.STRING, @"""\U00000000"""),
		Tuple.Create(TokenType.STRING, @"""\U0000ffAB"""),
		Tuple.Create(TokenType.STRING, @"""" + F100 + @""""),
	}},
    { "number", new [] {
		Tuple.Create(TokenType.NUMBER, "0"),
		Tuple.Create(TokenType.NUMBER, "1"),
		Tuple.Create(TokenType.NUMBER, "9"),
		Tuple.Create(TokenType.NUMBER, "42"),
		Tuple.Create(TokenType.NUMBER, "1234567890"),
		Tuple.Create(TokenType.NUMBER, "00"),
		Tuple.Create(TokenType.NUMBER, "01"),
		Tuple.Create(TokenType.NUMBER, "07"),
		Tuple.Create(TokenType.NUMBER, "042"),
		Tuple.Create(TokenType.NUMBER, "01234567"),
		Tuple.Create(TokenType.NUMBER, "0x0"),
		Tuple.Create(TokenType.NUMBER, "0x1"),
		Tuple.Create(TokenType.NUMBER, "0xf"),
		Tuple.Create(TokenType.NUMBER, "0x42"),
		Tuple.Create(TokenType.NUMBER, "0x123456789abcDEF"),
		Tuple.Create(TokenType.NUMBER, "0x" + F100),
		Tuple.Create(TokenType.NUMBER, "0X0"),
		Tuple.Create(TokenType.NUMBER, "0X1"),
		Tuple.Create(TokenType.NUMBER, "0XF"),
		Tuple.Create(TokenType.NUMBER, "0X42"),
		Tuple.Create(TokenType.NUMBER, "0X123456789abcDEF"),
		Tuple.Create(TokenType.NUMBER, "0X" + F100),
		Tuple.Create(TokenType.NUMBER, "-0"),
		Tuple.Create(TokenType.NUMBER, "-1"),
		Tuple.Create(TokenType.NUMBER, "-9"),
		Tuple.Create(TokenType.NUMBER, "-42"),
		Tuple.Create(TokenType.NUMBER, "-1234567890"),
		Tuple.Create(TokenType.NUMBER, "-00"),
		Tuple.Create(TokenType.NUMBER, "-01"),
		Tuple.Create(TokenType.NUMBER, "-07"),
		Tuple.Create(TokenType.NUMBER, "-29"),
		Tuple.Create(TokenType.NUMBER, "-042"),
		Tuple.Create(TokenType.NUMBER, "-01234567"),
		Tuple.Create(TokenType.NUMBER, "-0x0"),
		Tuple.Create(TokenType.NUMBER, "-0x1"),
		Tuple.Create(TokenType.NUMBER, "-0xf"),
		Tuple.Create(TokenType.NUMBER, "-0x42"),
		Tuple.Create(TokenType.NUMBER, "-0x123456789abcDEF"),
		Tuple.Create(TokenType.NUMBER, "-0x" + F100),
		Tuple.Create(TokenType.NUMBER, "-0X0"),
		Tuple.Create(TokenType.NUMBER, "-0X1"),
		Tuple.Create(TokenType.NUMBER, "-0XF"),
		Tuple.Create(TokenType.NUMBER, "-0X42"),
		Tuple.Create(TokenType.NUMBER, "-0X123456789abcDEF"),
		Tuple.Create(TokenType.NUMBER, "-0X" + F100),
	}},
    { "float", new []{
		Tuple.Create(TokenType.FLOAT, "0."),
		Tuple.Create(TokenType.FLOAT, "1."),
		Tuple.Create(TokenType.FLOAT, "42."),
		Tuple.Create(TokenType.FLOAT, "01234567890."),
		Tuple.Create(TokenType.FLOAT, ".0"),
		Tuple.Create(TokenType.FLOAT, ".1"),
		Tuple.Create(TokenType.FLOAT, ".42"),
		Tuple.Create(TokenType.FLOAT, ".0123456789"),
		Tuple.Create(TokenType.FLOAT, "0.0"),
		Tuple.Create(TokenType.FLOAT, "1.0"),
		Tuple.Create(TokenType.FLOAT, "42.0"),
		Tuple.Create(TokenType.FLOAT, "01234567890.0"),
		Tuple.Create(TokenType.FLOAT, "0e0"),
		Tuple.Create(TokenType.FLOAT, "1e0"),
		Tuple.Create(TokenType.FLOAT, "42e0"),
		Tuple.Create(TokenType.FLOAT, "01234567890e0"),
		Tuple.Create(TokenType.FLOAT, "0E0"),
		Tuple.Create(TokenType.FLOAT, "1E0"),
		Tuple.Create(TokenType.FLOAT, "42E0"),
		Tuple.Create(TokenType.FLOAT, "01234567890E0"),
		Tuple.Create(TokenType.FLOAT, "0e+10"),
		Tuple.Create(TokenType.FLOAT, "1e-10"),
		Tuple.Create(TokenType.FLOAT, "42e+10"),
		Tuple.Create(TokenType.FLOAT, "01234567890e-10"),
		Tuple.Create(TokenType.FLOAT, "0E+10"),
		Tuple.Create(TokenType.FLOAT, "1E-10"),
		Tuple.Create(TokenType.FLOAT, "42E+10"),
		Tuple.Create(TokenType.FLOAT, "01234567890E-10"),
		Tuple.Create(TokenType.FLOAT, "01.8e0"),
		Tuple.Create(TokenType.FLOAT, "1.4e0"),
		Tuple.Create(TokenType.FLOAT, "42.2e0"),
		Tuple.Create(TokenType.FLOAT, "01234567890.12e0"),
		Tuple.Create(TokenType.FLOAT, "0.E0"),
		Tuple.Create(TokenType.FLOAT, "1.12E0"),
		Tuple.Create(TokenType.FLOAT, "42.123E0"),
		Tuple.Create(TokenType.FLOAT, "01234567890.213E0"),
		Tuple.Create(TokenType.FLOAT, "0.2e+10"),
		Tuple.Create(TokenType.FLOAT, "1.2e-10"),
		Tuple.Create(TokenType.FLOAT, "42.54e+10"),
		Tuple.Create(TokenType.FLOAT, "01234567890.98e-10"),
		Tuple.Create(TokenType.FLOAT, "0.1E+10"),
		Tuple.Create(TokenType.FLOAT, "1.1E-10"),
		Tuple.Create(TokenType.FLOAT, "42.1E+10"),
		Tuple.Create(TokenType.FLOAT, "01234567890.1E-10"),
		Tuple.Create(TokenType.FLOAT, "-0.0"),
		Tuple.Create(TokenType.FLOAT, "-1.0"),
		Tuple.Create(TokenType.FLOAT, "-42.0"),
		Tuple.Create(TokenType.FLOAT, "-01234567890.0"),
		Tuple.Create(TokenType.FLOAT, "-0e0"),
		Tuple.Create(TokenType.FLOAT, "-1e0"),
		Tuple.Create(TokenType.FLOAT, "-42e0"),
		Tuple.Create(TokenType.FLOAT, "-01234567890e0"),
		Tuple.Create(TokenType.FLOAT, "-0E0"),
		Tuple.Create(TokenType.FLOAT, "-1E0"),
		Tuple.Create(TokenType.FLOAT, "-42E0"),
		Tuple.Create(TokenType.FLOAT, "-01234567890E0"),
		Tuple.Create(TokenType.FLOAT, "-0e+10"),
		Tuple.Create(TokenType.FLOAT, "-1e-10"),
		Tuple.Create(TokenType.FLOAT, "-42e+10"),
		Tuple.Create(TokenType.FLOAT, "-01234567890e-10"),
		Tuple.Create(TokenType.FLOAT, "-0E+10"),
		Tuple.Create(TokenType.FLOAT, "-1E-10"),
		Tuple.Create(TokenType.FLOAT, "-42E+10"),
		Tuple.Create(TokenType.FLOAT, "-01234567890E-10"),
		Tuple.Create(TokenType.FLOAT, "-01.8e0"),
		Tuple.Create(TokenType.FLOAT, "-1.4e0"),
		Tuple.Create(TokenType.FLOAT, "-42.2e0"),
		Tuple.Create(TokenType.FLOAT, "-01234567890.12e0"),
		Tuple.Create(TokenType.FLOAT, "-0.E0"),
		Tuple.Create(TokenType.FLOAT, "-1.12E0"),
		Tuple.Create(TokenType.FLOAT, "-42.123E0"),
		Tuple.Create(TokenType.FLOAT, "-01234567890.213E0"),
		Tuple.Create(TokenType.FLOAT, "-0.2e+10"),
		Tuple.Create(TokenType.FLOAT, "-1.2e-10"),
		Tuple.Create(TokenType.FLOAT, "-42.54e+10"),
		Tuple.Create(TokenType.FLOAT, "-01234567890.98e-10"),
		Tuple.Create(TokenType.FLOAT, "-0.1E+10"),
		Tuple.Create(TokenType.FLOAT, "-1.1E-10"),
		Tuple.Create(TokenType.FLOAT, "-42.1E+10"),
		Tuple.Create(TokenType.FLOAT, "-01234567890.1E-10"),
	}}};
        #endregion
    }
}
