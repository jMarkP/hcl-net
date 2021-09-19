using hcl_net.Parse.HCL;
using NUnit.Framework;

namespace hcl_net.Test.Parse.HCL
{
    [TestFixture]
    class TokenTests
    {
        [TestCase(TokenType.BOOL, "true", true, TestName = "BOOL:true")]
        [TestCase(TokenType.BOOL, "false", false, TestName = "BOOL:false")]
        [TestCase(TokenType.FLOAT, "3.14", (double)3.14, TestName = "FLOAT")]
        [TestCase(TokenType.NUMBER, "42", (long)42, TestName = "NUMBER")]
        [TestCase(TokenType.IDENT, "foo", "foo", TestName = "IDENT")]
        [TestCase(TokenType.STRING, @"""foo""", "foo", TestName = "STRING:foo")]
        [TestCase(TokenType.STRING, @"""foo\nbar""", "foo\nbar", TestName = "STRING:foo`nbar")]
        [TestCase(TokenType.STRING, @"""${file(""foo"")}""", "${file(\"foo\")}", TestName = "STRING:file")]
        [TestCase(TokenType.STRING, @"""${replace(var.foo, ""."", ""\\."")}""", "${replace(var.foo, \".\", \"\\\\.\")}", TestName = "STRING:replace")]
        [TestCase(TokenType.HEREDOC, "<<EOF\nfoo\nbar\nEOF\n", "foo\nbar\n", TestName = "HEREDOC:EOF")]
        [TestCase(TokenType.HEREDOC, "<<-EOF\n\t\t foo\n\t\t  bar\n\t\t EOF\n", "foo\n bar\n", TestName = "HEREDOC:-EOF")]
        [TestCase(TokenType.HEREDOC, "<<-EOF\n\t\t foo\n\tbar\n\t\t EOF\n", "\t\t foo\n\tbar\n", TestName = "HEREDOC:-EOF (extra whitespace)")]
        public void TestTokenValue(TokenType type, string text, object value)
        {
            var SUT = new Token(type, new Pos(), text, false);
            Assert.That(SUT.Value, Is.EqualTo(value));
        }
    }
}
