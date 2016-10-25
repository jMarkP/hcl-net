using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using hcl_net.Utilities;
using NUnit.Framework.Internal;
using NUnit.Framework;

namespace hcl_net.Test
{
    [TestFixture]
    class HclQuoteUtilTests
    {
        [TestCaseSource("Unquote")]
        public void TestUnquote(KeyValuePair<string, string> testCase)
        {
            var input = testCase.Key;
            var expected = testCase.Value;

            string error;
            var actual = HclQuoteUtil.Unquote(input, out error);
            Assert.That(error, Is.Null);
            Assert.That(actual, Is.EqualTo(expected));
        }

        private static KeyValuePair<string, string>[] Unquote()
        {
            return new Dictionary<string, string>
            {
                {@"""""", ""},
                {@"""a""", "a"},
                {@"""abc""", "abc"},
                {@"""☺""", "☺"},
                {@"""hello world""", "hello world"},
                {@"""\xFF""", "\xFF"},
                {@"""\377""", "\0377"},
                {@"""\u1234""", "\u1234"},
                {@"""\U00010111""", "\U00010111"},
                {@"""\U0001011111""", "\U0001011111"},
                {@"""\a\b\f\n\r\t\v\\\""""", "\a\b\f\n\r\t\v\\\""},
                {@"""'""", "'"},
                {@"""${file(""foo"")}""", "${file(\"foo\")}"},
                {@"""${file(""\""foo\"""")}""", "${file(\"\\\"foo\\\"\")}"},
                {@"""echo ${var.region}${element(split("","",var.zones),0)}""",
                    "echo ${var.region}${element(split(\",\",var.zones),0)}"},
                {@"""${HH\\:mm\\:ss}""", "${HH\\:mm\\:ss}"},
            }.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).ToArray();
        }
    }
}
