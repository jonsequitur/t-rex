using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering;
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
            var rootCommand = new RootCommand(description: "A command line testing tool for .NET")
                              {
                                  new Option("--file",
                                             ".trx file(s) to parse",
                                             new Argument<FileInfo[]>().ExistingFilesOnly()),
                                  new Option(new[] { "-f", "--filter" },
                                             "Only look at tests containing the specified text. \"*\" can be used as a wildcard.",
                                             new Argument<string>()),
                                  new Option("--format",
                                             "The format for the output. (Summary, JSON)",
                                             new Argument<OutputFormat>(OutputFormat.Summary)),
                                  new Option("--path",
                                             "Directory or directories to search for .trx files. Only the most recent .trx file in a given directory is used.",
                                             new Argument<DirectoryInfo[]>(new[] { new DirectoryInfo(Directory.GetCurrentDirectory()) })),
                                  new Option(new[] { "-d", "--hide-test-output" },
                                             "For failed tests, hide detailed test output. (Defaults to false.)",
                                             new Argument<bool>()),
                                  new Option("--ansi-mode",
                                             "Shows output formatted using ANSI sequences",
                                             new Argument<bool>()),
                                  new Option("--virtual-terminal-mode",
                                             "Shows output formatted using ANSI sequences",
                                             new Argument<bool>())
                              };

            rootCommand.Handler = CommandHandler.Create(typeof(CommandLine).GetMethod(nameof(DisplayLastTestRunResults)));

            Parser = new CommandLineBuilder(rootCommand)
                     .UseDefaults()
                     .Build();
        }

        public static async Task<int> DisplayLastTestRunResults(
            OutputFormat format,
            FileInfo[] file,
            DirectoryInfo[] path,
            string filter,
            bool ansiMode = false,
            bool hideTestOutput = false,
            bool virtualTerminalMode = false,
            IConsole console = null)
        {
            if (virtualTerminalMode)
            {
                console.TryEnableVirtualTerminal();
            }

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
                        allFiles.AddRange(SearchDirectory(directoryInfo.FullName));
                    }
                }
            }

            var resultSet = Create(allFiles);

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
                    if (!ansiMode)
                    {
                        var view = new SummaryView(hideTestOutput);
                        await view.WriteAsync(console, resultSet);
                    }
                    else
                    {
                        var view = new AnsiSummaryView(hideTestOutput, resultSet);
                        view.Render(new ConsoleRenderer(console), console.GetRegion());
                    }

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

        private static TestResultSet Create(
            IEnumerable<FileInfo> files)
        {
            var testResults = new List<TestResult>();

            foreach (var file in files)
            {
                testResults.AddRange(file.Parse());
            }

            return new TestResultSet(testResults);
        }

        private static IEnumerable<FileInfo> SearchDirectory(string path)
        {
            var allFiles = new DirectoryInfo(path)
                           .GetFiles("*.trx", SearchOption.AllDirectories)
                           .GroupBy(f => f.Directory.FullName);

            foreach (var folder in allFiles)
            {
                yield return folder.OrderBy(f => f.LastWriteTime).Last();
            }
        }
    }
}