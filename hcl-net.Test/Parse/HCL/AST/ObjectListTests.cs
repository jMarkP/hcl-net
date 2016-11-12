using System;
using System.Linq;
using hcl_net.Parse.HCL;
using hcl_net.Parse.HCL.AST;
using NUnit.Framework;

namespace hcl_net.Test.Parse.HCL.AST
{
    [TestFixture]
    class ObjectListTests
    {
        [TestCaseSource("FilterTestCases")]
        public void Filter_FiltersItems(Tuple<string, ObjectItem[], ObjectItem[]> testCase)
        {
            var input = new ObjectList(testCase.Item2);
            var expected = new ObjectList(testCase.Item3);
            var actual = input.Filter(testCase.Item1);
            CompareKeys(actual, expected);
        }

        private static Tuple<string, ObjectItem[], ObjectItem[]>[] FilterTestCases()
        {
            return new[]
            {
                Tuple.Create("foo",
                    new[]
                    {
                        BuildObjectItem("foo"),
                    },
                    new[]
                    {
                        BuildObjectItem()
                    }),
                Tuple.Create("foo",
                    new[]
                    {
                        BuildObjectItem("foo", "bar"),
                        BuildObjectItem("baz")
                    },
                    new[]
                    {
                        BuildObjectItem("bar")
                    })
            };
        }

        private static ObjectItem BuildObjectItem(params string[] keys)
        {
            return new ObjectItem(keys.Select(k => new ObjectKey(new Token(TokenType.STRING, default(Pos), '"' + k + '"', false))),
                default(Pos), null, null, null);
        }

        private void CompareKeys(ObjectList actual, ObjectList expected)
        {
            Assert.That(actual.Items.Length, Is.EqualTo(expected.Items.Length));
            for (var i = 0; i < expected.Items.Length; i++)
            {
                CompareKeys(actual.Items[i], expected.Items[i]);
            }
        }

        private void CompareKeys(ObjectItem actual, ObjectItem expected)
        {
            Assert.That(actual.Keys.Length, Is.EqualTo(expected.Keys.Length));
            for (var i = 0; i < expected.Keys.Length; i++)
            {
                Assert.That(expected.Keys[i].Token.Value as string,
                    Is.EqualTo(actual.Keys[i].Token.Value as string));
            }
        }
    }
}
