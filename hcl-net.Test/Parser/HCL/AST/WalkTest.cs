using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using hcl_net.Parser.HCL;
using hcl_net.Parser.HCL.AST;
using NUnit.Framework;

namespace hcl_net.Test.Parser.HCL.AST
{
    [TestFixture]
    class WalkTest
    {
        [Test]
        public void Walk_VisitsNodesInCorrectOrder()
        {
            var items = new[]
            {
                new ObjectItem(new[]
                    {
                        new ObjectKey(new Token(TokenType.STRING, default(Pos), "\"foo\"", false)),
                        new ObjectKey(new Token(TokenType.STRING, default(Pos), "\"bar\"", false)),
                    },
                    default(Pos),
                    new LiteralType(new Token(TokenType.STRING, default(Pos), "\"example\"", false), null, null),
                    null, null
                ),
                new ObjectItem(new []
                    {
                        new ObjectKey(new Token(TokenType.STRING, default(Pos), "\"baz\"", false)) 
                    },
                    default(Pos), null, null, null), 
            };

            var node = new ObjectList(items);

            var expectedOrder = new[]
            {
                "ObjectList",
                "ObjectItem",
                "ObjectKey",
                "ObjectKey",
                "LiteralType",
                "ObjectItem",
                "ObjectKey"
            };

            var count = 0;
            node.Walk((INode n, out INode rewritten) =>
            {
                if (n == null)
                {
                    rewritten = n;
                    return false;
                }
                var typeName = n.GetType().Name;
                Assert.That(typeName, Is.EqualTo(expectedOrder[count]));
                count++;
                rewritten = n;
                return true;
            });
        }

        [Test]
        public void Walk_TestEquality()
        {
            var items = new[]
            {
                new ObjectItem(new[]
                    {
                        new ObjectKey(new Token(TokenType.STRING, default(Pos), "\"foo\"", false))
                    },
                    default(Pos),
                    null, null, null
                ),
                new ObjectItem(new []
                    {
                        new ObjectKey(new Token(TokenType.STRING, default(Pos), "\"bar\"", false))
                    },
                    default(Pos), null, null, null),
            };
            var node = new ObjectList(items);

            var rewritten = node.Walk((INode n, out INode nn) =>
            {
                nn = n;
                return true;
            });
            Assert.That(rewritten, Is.EqualTo(node));
        }

        [Test]
        public void Walk_TestRewrite()
        {
            var items = new[]
            {
                new ObjectItem(new[]
                    {
                        new ObjectKey(new Token(TokenType.STRING, default(Pos), "\"foo\"", false)),
                        new ObjectKey(new Token(TokenType.STRING, default(Pos), "\"bar\"", false))
                    },
                    default(Pos),
                    null, null, null
                ),
                new ObjectItem(new []
                    {
                        new ObjectKey(new Token(TokenType.STRING, default(Pos), "\"baz\"", false))
                    },
                    default(Pos), null, null, null),
            };
            var node = new ObjectList(items);

            const string suffix = "_example";
            var actual = node.Walk((INode n, out INode rewritten) =>
            {
                var objectKey = n as ObjectKey;
                if (objectKey != null)
                {
                    rewritten = new ObjectKey(
                        new Token(TokenType.STRING, 
                            default(Pos), 
                            objectKey.Token.Text + suffix, 
                            false));
                    return true;
                }
                rewritten = n;
                return true;
            });

            actual.Walk((INode n, out INode rewritten) =>
            {
                rewritten = n;
                var objectKey = n as ObjectKey;
                if (objectKey != null)
                {
                    Assert.That(objectKey.Token.Text, Is.StringEnding(suffix));
                }
                return true;
            });
        }
    }
}
