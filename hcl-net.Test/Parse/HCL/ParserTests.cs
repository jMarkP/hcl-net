using System;
using System.Linq;
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

        [TestCase("foo = [\"123\", 123]", new [] {TokenType.STRING, TokenType.NUMBER})]
        [TestCase("foo = [123, \"123\",]", new[] { TokenType.NUMBER, TokenType.STRING })]
        [TestCase("foo = []", new TokenType[0])]
        [TestCase(@"foo = [1,
""string"",
<<EOF
heredoc contents
EOF
]", new[] { TokenType.NUMBER, TokenType.STRING, TokenType.HEREDOC })]

        public void TestListOfLiterals(string input, TokenType[] expectedTypes)
        {
            var sut = new Parser(input);
            string parserError;
            var item = sut.ParseObjectItem(out parserError);
            Assert.That(parserError, Is.Null);
            Assert.That(item.Val, Is.InstanceOf<ListType>());
            var tokens = ((ListType) item.Val).List
                .Cast<LiteralType>()
                .Select(i => i.Token.Type)
                .ToArray();
            Assert.That(tokens, Is.EquivalentTo(expectedTypes));
        }

        [Test]
        public void TestListOfMaps()
        {
            var input = @"foo = [
    {key = ""bar""},
    {key = ""baz"", key2 = ""qux""},
]";
            var sut = new Parser(input);
            string parserError;
            var file = sut.Parse(out parserError);
            Assert.That(parserError, Is.Null);

            var expectedKeys = new[] {@"""bar""", @"""baz""", @"""qux"""};
            Assert.That(file.Node, Is.InstanceOf<ObjectList>());
            var objectList = (ObjectList)file.Node;
            var firstItem = objectList.Items[0]; // i.e. foo = [..]
            Assert.That(firstItem.Val, Is.InstanceOf<ListType>());
            var list = (ListType) firstItem.Val;
            var actualKeys = list.List
                .SelectMany(l => ((ObjectType) l).List.Items)
                .Select(item => ((LiteralType) item.Val).Token.Text)
                .ToArray();
            Assert.That(actualKeys, Is.EquivalentTo(expectedKeys));
        }

        [Test]
        public void TestListOfMaps_RequiresComma()
        {
            var input = @"foo = [
    {key = ""bar""}
    {key = ""baz""}
]";
            var sut = new Parser(input);
            string parserError;
            var result = sut.Parse(out parserError);
            Assert.That(result, Is.Null);
            Assert.That(parserError, Is.EqualTo("Error parsing list. Expected comma or list end, got: LBRACE"));
        }
    }
}
