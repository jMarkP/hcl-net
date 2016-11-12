using System;
using hcl_net.Parse.HCL;
using hcl_net.Parse.HCL.AST;
using NUnit.Framework;

namespace hcl_net.Test.Parse.HCL
{
    [TestFixture]
    class ParserTests
    {
        [TestCase("foo = \"foo\"", TokenType.STRING)]
        [TestCase("foo = 123", TokenType.NUMBER)]
        [TestCase("foo = -29", TokenType.NUMBER)]
        [TestCase("foo = 123.12", TokenType.FLOAT)]
        [TestCase("foo = -123.12", TokenType.FLOAT)]
        [TestCase("foo = true", TokenType.BOOL)]
        [TestCase("foo = <<EOF\nHello\nWorld\nEOF", TokenType.HEREDOC)]
        public void TestLiteralTypeParsing(string input, TokenType expectedType)
        {
            var sut = new Parser(input);
            string parserError;
            var item = sut.ParseObjectItem(out parserError);
            Assert.That(parserError, Is.Null);
            Assert.That(item.Val, Is.InstanceOf<LiteralType>());
            Assert.That(((LiteralType)item.Val).Token.Type, Is.EqualTo(expectedType));
        }
    }
}
