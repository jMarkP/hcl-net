using hcl_net.Parse.HCL;
using NUnit.Framework;

namespace hcl_net.Test.Parse.HCL
{
    [TestFixture]
    class TokenTests
    {
        [TestCase(TokenType.BOOL, "true", true)]
        [TestCase(TokenType.BOOL, "false", false)]
        [TestCase(TokenType.FLOAT, "3.14", (double)3.14)]
        [TestCase(TokenType.NUMBER, "42", (long)42)]
        [TestCase(TokenType.IDENT, "foo", "foo")]
        [TestCase(TokenType.STRING, @"""foo""", "foo")]
        [TestCase(TokenType.STRING, @"""foo\nbar""", "foo\nbar")]
        [TestCase(TokenType.STRING, @"""${file(""foo"")}""", "${file(\"foo\")}")]
        [TestCase(TokenType.HEREDOC, "<<EOF\nfoo\nbar\nEOF", "foo\nbar")]
        [TestCase(TokenType.HEREDOC, "<<-EOF\n\t\t foo\n\t\t  bar\n\t\t EOF", "foo\n bar\n")]
        [TestCase(TokenType.HEREDOC, "<<-EOF\n\t\t foo\n\tbar\n\t\t EOF", "\t\t foo\n\tbar\n")]
        public void TestTokenValue(TokenType type, string text, object value)
        {
            var SUT = new Token(type, new Pos(), text, false);
            Assert.That(SUT.Value, Is.EqualTo(value));
        }
    }
}
