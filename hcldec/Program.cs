using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using hcl_net.v2.hclsyntax;

namespace hcldec
{
    class Program
    {
        static Task Main(string[] args)
        {
            var outOption = new Option<string>(
                "--out",
                description: "write to the given file, instead of stdout");
            outOption.AddAlias("-o");
            var specOption = new Option<string>(
                "--spec",
                description: "path to spec file (required)");
            specOption.AddAlias("-s");
            var extraVarsOption = new Option<string>(
                "--vars",
                description: "provide variables to the given configuration file(s)"
            );
            extraVarsOption.AddAlias("-V");
            var showVersionOption = new Option<bool>(
                "--version",
                description: "show the version number and immediately exit");
            showVersionOption.AddAlias("-v");
            
            var rootCommand = new RootCommand
            {
                outOption,
                specOption,
                extraVarsOption,
                //showVersionOption,
            };

            rootCommand.Handler = CommandHandler.Create<string, string, string>(
                Run);

            return rootCommand.InvokeAsync(args);
        }

        private static async Task<int> Run(string outPath, string specPath, string extraVars)
        {
            // if (string.IsNullOrEmpty(specPath))
            // {
            //     await Console.Error.WriteLineAsync("ERR: Must specify path to spec file");
            //     return 1;
            // }

            var contents = @"object {
  attr ""name"" {
            type     = string
            required = true
        }
        attr ""is_member"" {
            type = bool
        }
    }";

            //var f = new hcl_net.v2.hclsyntax.parser.Parser(contents).Parse(out var err);
            var tokens = Scanner
                .ScanTokens(Encoding.UTF8.GetBytes(contents), "test", Pos.CreateForFile("test"), ScanMode.Normal)
                .ToArray(); 
            return 0;
        }
    }
}