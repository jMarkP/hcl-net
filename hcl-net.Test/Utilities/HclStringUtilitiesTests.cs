using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using hcl_net.Utilities;
using NUnit.Framework;

namespace hcl_net.Test.Utilities
{
    [TestFixture]
    class HclStringUtilitiesTests
    {
        [TestCaseSource(nameof(Unquote))]
        public void Unquote_ProcessesStringsCorrectly(KeyValuePair<string, string> testCase)
        {
            var input = testCase.Key;
            var expected = testCase.Value;

            string error;
            var actual = input.UnquoteHclString(out error);
            Assert.That(error, Is.Null);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Unquote_RejectsInvalidStrings()
        {
            foreach (var invalidString in InvalidStrings())
            {
                TestInvalidString(invalidString);
            }
        }

        private static void TestInvalidString(string invalidString)
        {
            string error;
            var result = invalidString.UnquoteHclString(out error);
            Assert.That(result, Is.EqualTo(string.Empty), invalidString);
            Assert.That(result, Is.Not.Null, invalidString);
        }

        private static TestCaseData[] Unquote()
        {
            return new Dictionary<string, string>
            {
                {@"""""", ""},
                {@"""a""", "a"},
                {@"""abc""", "abc"},
                {@"""☺""", "☺"},
                {@"""hello world""", "hello world"},
                {@"""\xFF""", "\xFF"},
                {@"""\377""", "\xFF"},
                {@"""\u1234""", "\u1234"},
                {@"""\U00010111""", "\U00010111"},
                {@"""\U0001011111""", "\U0001011111"},
                {@"""\a\b\f\n\r\t\v\\\""""", "\a\b\f\n\r\t\v\\\""},
                {@"""'""", "'"},
                {@"""${file(""foo"")}""", "${file(\"foo\")}"},
                {@"""${file(""\""foo\"""")}""", "${file(\"\\\"foo\\\"\")}"},
                {@"""echo ${var.region}${element(split("","",var.zones),0)}""",
                    "echo ${var.region}${element(split(\",\",var.zones),0)}"},
                {@"""${HH\:mm\:ss}""", "${HH\\:mm\\:ss}"},
                {@"""\a\b\f\r\n\t\v""", "\a\b\f\r\n\t\v" },
                {@"""\\""", "\\" },
                {@"""abc\xffdef""", "abc\x00ffdef" },
                {@"""\u263a""", "\u263a" },
                {@"""\U0010ffff""","\U0010ffff"},
                {@"""\x04""", "\x04"}
            }.Select(x => new KeyValuePair<string, string>(x.Key, x.Value))
                .Select((x, i) => new TestCaseData(x).SetName($"{i:D2}"))
                .ToArray();
        }

        private static string[] InvalidStrings()
        {
            using (var stream = typeof(HclStringUtilitiesTests).Assembly.GetManifestResourceStream("hcl_net.Test.Utilities.InvalidStrings.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd()
                    .Split(new []{"\r\n" }, StringSplitOptions.None)
                    .Select(x => x.Replace(@"\n", "\n"))
                    .ToArray();
            }

        }
    }
}
