using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TRexLib;

namespace TRex.CommandLine
{
    public static class CommandLine
    {
        public static Parser Parser { get; }

        static CommandLine()
        {
            var rootCommand = new RootCommand("A command line testing tool for .NET")
            {
                new Option("--file", ".trx file(s) to parse")
                {
                    Argument = new Argument<FileInfo[]>("filename(s)").ExistingOnly()
                },

                new Option(new[] { "-f", "--filter" },
                           "Only look at tests containing the specified text. \"*\" can be used as a wildcard.")
                {
                    Argument = new Argument<string>("testname")
                },

                new Option("--format", "The format for the output.")
                {
                    Argument = new Argument<OutputFormat>(() => OutputFormat.Summary)
                },

                new Option("--path",
                           "Directory or directories to search for .trx files. Only the most recent .trx file in a given directory is used.")
                {
                    Argument = new Argument<DirectoryInfo[]>("dir", getDefaultValue: () => new[] { new DirectoryInfo(Directory.GetCurrentDirectory()) })
                },

                new Option(new[] { "-d", "--hide-test-output" },
                           "For failed tests, hide detailed test output.")
                {
                    Argument = new Argument<bool>()
                }
            };

            rootCommand.Handler = CommandHandler.Create(typeof(CommandLine).GetMethod(nameof(DisplayResults)));

            Parser = new CommandLineBuilder(rootCommand)
                     .UseDefaults()
                     .Build();
        }

        public static async Task<int> DisplayResults(
            OutputFormat format,
            FileInfo[] file,
            DirectoryInfo[] path,
            string filter,
            bool hideTestOutput = false,
            IConsole console = null)
        {
            var allFiles = new List<FileInfo>();

            if (file != null && file.Any())
            {
                foreach (var fileInfo in file.Where(f => f.Exists))
                {
                    allFiles.Add(fileInfo);
                }
            }
            else
            {
                foreach (var directoryInfo in path)
                {
                    if (directoryInfo.Exists)
                    {
                        allFiles.AddRange(TestResultSet.FindTrxFiles(directoryInfo.FullName));
                    }
                }
            }

            var resultSet = TestResultSet.Create(allFiles);

            if (!string.IsNullOrWhiteSpace(filter))
            {
                filter = $"*{filter}*";

                var regex = new Regex($"^{filter.Replace("*", ".*")}$", RegexOptions.IgnoreCase);

                resultSet = new TestResultSet(
                    resultSet.Where(r => regex.IsMatch(r.FullyQualifiedTestName)));
            }

            switch (format)
            {
                case OutputFormat.Summary:
                {
                    var view = new SummaryView(hideTestOutput);
                    await view.WriteAsync(console, resultSet);

                    break;
                }

                case OutputFormat.Json:
                {
                    var view = new JsonView();
                    await view.WriteAsync(console, resultSet);
                    break;
                }
            }

            if (resultSet.Failed.Any())
            {
                return 1;
            }
            else if (!resultSet.Any())
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}