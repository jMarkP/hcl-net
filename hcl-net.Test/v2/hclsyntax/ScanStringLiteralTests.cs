using System.Linq;
using System.Text;
using hcl_net.v2.hclsyntax;
using NUnit.Framework;

namespace hcl_net.Test.v2.hclsyntax
{
    public class ScanStringLiteralTests
    {
	    [TestCaseSource(nameof(TestCases))]
	    public void TestScanStringLiteral(TestCase testCase)
	    {
		    var actualQuoted =
			    Scanner.ScanStringLiteral(Encoding.UTF8.GetBytes(testCase.InputString), true);
		    var actualUnquoted = 
			    Scanner.ScanStringLiteral(Encoding.UTF8.GetBytes(testCase.InputString), false);
		    
		    Assert.That(actualQuoted, Is.EquivalentTo(testCase.ExpectedQuoted.Select(Encoding.UTF8.GetBytes)), "Quoted");
		    Assert.That(actualUnquoted, Is.EquivalentTo(testCase.ExpectedUnquoted.Select(Encoding.UTF8.GetBytes)), "Unquoted");
	    }
	    
        private static TestCaseData[] TestCases()
        {
	        return new []
	        {
		        new TestCase
		        {
			        InputString = "",
			        ExpectedQuoted = new string[] { },
			        ExpectedUnquoted = new string[] { },
		        },
		        new TestCase
		        {
			        InputString = "hello",
			        ExpectedQuoted = new string[] {"hello"},
			        ExpectedUnquoted = new string[] {"hello"},
		        },
		        new TestCase
		        {
			        InputString = "hello world",
			        ExpectedQuoted = new string[] {"hello world"},
			        ExpectedUnquoted = new string[] {"hello world"},
		        },
		        new TestCase
		        {
			        InputString = "hello\\nworld",
			        ExpectedQuoted = new string[] {"hello", "\\n", "world"},
			        ExpectedUnquoted = new string[] {"hello\\nworld"},
		        },
		        new TestCase
		        {
			        InputString = "hello\\🥁world",
			        ExpectedQuoted = new string[] {"hello", "\\🥁", "world"},
			        ExpectedUnquoted = new string[] {"hello\\🥁world"},
		        },
		        new TestCase
		        {
			        InputString = "hello\\uabcdworld",
			        ExpectedQuoted = new string[] {"hello", "\\uabcd", "world"},
			        ExpectedUnquoted = new string[] {"hello\\uabcdworld"},
		        },
		        new TestCase
		        {
			        InputString = "hello\\uabcdabcdworld",
			        ExpectedQuoted = new string[] {"hello", "\\uabcd", "abcdworld"},
			        ExpectedUnquoted = new string[] {"hello\\uabcdabcdworld"},
		        },
		        new TestCase
		        {
			        InputString = "hello\\uabcworld",
			        ExpectedQuoted = new string[] {"hello", "\\uabc", "world"},
			        ExpectedUnquoted = new string[] {"hello\\uabcworld"},
		        },
		        new TestCase
		        {
			        InputString = "hello\\U01234567world",
			        ExpectedQuoted = new string[] {"hello", "\\U01234567", "world"},
			        ExpectedUnquoted = new string[] {"hello\\U01234567world"},
		        },
		        new TestCase
		        {
			        InputString = "hello\\U012345670123world",
			        ExpectedQuoted = new string[] {"hello", "\\U01234567", "0123world"},
			        ExpectedUnquoted = new string[] {"hello\\U012345670123world"},
		        },
		        new TestCase
		        {
			        InputString = "hello\\Uabcdworld",
			        ExpectedQuoted = new string[] {"hello", "\\Uabcd", "world"},
			        ExpectedUnquoted = new string[] {"hello\\Uabcdworld"},
		        },
		        new TestCase
		        {
			        InputString = "hello\\Uabcworld",
			        ExpectedQuoted = new string[] {"hello", "\\Uabc", "world"},
			        ExpectedUnquoted = new string[] {"hello\\Uabcworld"},
		        },
		        new TestCase
		        {
			        InputString = "hello\\uworld",
			        ExpectedQuoted = new string[] {"hello", "\\u", "world"},
			        ExpectedUnquoted = new string[] {"hello\\uworld"},
		        },
		        new TestCase
		        {
			        InputString = "hello\\Uworld",
			        ExpectedQuoted = new string[] {"hello", "\\U", "world"},
			        ExpectedUnquoted = new string[] {"hello\\Uworld"},
		        },
		        new TestCase
		        {
			        InputString = "hello\\u",
			        ExpectedQuoted = new string[] {"hello", "\\u"},
			        ExpectedUnquoted = new string[] {"hello\\u"},
		        },
		        new TestCase
		        {
			        InputString = "hello\\U",
			        ExpectedQuoted = new string[] {"hello", "\\U"},
			        ExpectedUnquoted = new string[] {"hello\\U"},
		        },
		        new TestCase
		        {
			        InputString = "hello\\",
			        ExpectedQuoted = new string[] {"hello", "\\"},
			        ExpectedUnquoted = new string[] {"hello\\"},
		        },
		        new TestCase
		        {
			        InputString = "hello$${world}",
			        ExpectedQuoted = new string[] {"hello", "$${", "world}"},
			        ExpectedUnquoted = new string[] {"hello", "$${", "world}"},
		        },
		        new TestCase
		        {
			        InputString = "hello$$world",
			        ExpectedQuoted = new string[] {"hello", "$$", "world"},
			        ExpectedUnquoted = new string[] {"hello", "$$", "world"},
		        },
		        new TestCase
		        {
			        InputString = "hello$world",
			        ExpectedQuoted = new string[] {"hello", "$", "world"},
			        ExpectedUnquoted = new string[] {"hello", "$", "world"},
		        },
		        new TestCase
		        {
			        InputString = "hello$",
			        ExpectedQuoted = new string[] {"hello", "$"},
			        ExpectedUnquoted = new string[] {"hello", "$"},
		        },
		        new TestCase
		        {
			        InputString = "hello$${",
			        ExpectedQuoted = new string[] {"hello", "$${"},
			        ExpectedUnquoted = new string[] {"hello", "$${"},
		        },
		        new TestCase
		        {
			        InputString = "hello%%{world}",
			        ExpectedQuoted = new string[] {"hello", "%%{", "world}"},
			        ExpectedUnquoted = new string[] {"hello", "%%{", "world}"},
		        },
		        new TestCase
		        {
			        InputString = "hello%%world",
			        ExpectedQuoted = new string[] {"hello", "%%", "world"},
			        ExpectedUnquoted = new string[] {"hello", "%%", "world"},
		        },
		        new TestCase
		        {
			        InputString = "hello%world",
			        ExpectedQuoted = new string[] {"hello", "%", "world"},
			        ExpectedUnquoted = new string[] {"hello", "%", "world"},
		        },
		        new TestCase
		        {
			        InputString = "hello%",
			        ExpectedQuoted = new string[] {"hello", "%"},
			        ExpectedUnquoted = new string[] {"hello", "%"},
		        },
		        new TestCase
		        {
			        InputString = "hello%%{",
			        ExpectedQuoted = new string[] {"hello", "%%{"},
			        ExpectedUnquoted = new string[] {"hello", "%%{"},
		        },
		        new TestCase
		        {
			        InputString = "hello\\${world}",
			        ExpectedQuoted = new string[] {"hello", "\\$", "{world}"},
			        ExpectedUnquoted = new string[] {"hello\\", "$", "{world}"},
		        },
		        new TestCase
		        {
			        InputString = "hello\\%{world}",
			        ExpectedQuoted = new string[] {"hello", "\\%", "{world}"},
			        ExpectedUnquoted = new string[] {"hello\\", "%", "{world}"},
		        },
		        new TestCase
		        {
			        InputString = "hello\nworld",
			        ExpectedQuoted = new string[] {"hello", "\n", "world"},
			        ExpectedUnquoted = new string[] {"hello", "\n", "world"},
		        },
		        new TestCase
		        {
			        InputString = "hello\rworld",
			        ExpectedQuoted = new string[] {"hello", "\r", "world"},
			        ExpectedUnquoted = new string[] {"hello", "\r", "world"},
		        },
		        new TestCase
		        {
			        InputString = "hello\r\nworld",
			        ExpectedQuoted = new string[] {"hello", "\r\n", "world"},
			        ExpectedUnquoted = new string[] {"hello", "\r\n", "world"},
		        },
	        }
		        .Select((tc, i) => new TestCaseData(tc).SetName($"{i:D2}"))
		        .ToArray();
        }

        public class TestCase
        {
            public string InputString { get; set; }
            public string[] ExpectedQuoted { get; set; }
            public string[] ExpectedUnquoted { get; set; }
        }
    }
}