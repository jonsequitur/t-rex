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
    public class CommandLine
    {
        public static Parser Parser { get; }

        static CommandLine()
        {
            var commandLine = new CommandLineBuilder()
                              
                              .UseParseDirective()
                              .UseSuggestDirective()
                              .UseParseErrorReporting()
                              .RegisterWithDotnetSuggest()
                              .UseExceptionHandler()

                              .UseHelp()

                              .AddOption("--file",
                                         ".trx file(s) to parse",
                                         a => a.ExistingFilesOnly()
                                               .ParseArgumentsAs<FileInfo[]>())
                              .AddOption("--filter",
                                         "Only look at tests containing the specified text. \"*\" can be used as a wildcard.",
                                         args => args.ExactlyOne())
                              .AddOption("--format",
                                         "The format for the output. (Summary, JSON)",
                                         args => args.WithDefaultValue(() => OutputFormat.Summary)
                                                     .ParseArgumentsAs<OutputFormat>())
                              .AddOption("--path",
                                         "Directory or directories to search for .trx files. Only the most recent .trx file in a given directory is used.",
                                         a => a.WithDefaultValue(Directory.GetCurrentDirectory)
                                               .ParseArgumentsAs<DirectoryInfo[]>())
                              .AddOption("--hide-test-output",
                                         "For failed tests, hide detailed test output. (Defaults to false.)",
                                         a => a.ParseArgumentsAs<bool>())
                              .AddOption("--ansi-mode",
                                         "Shows output formatted using ANSI sequences",
                                         a => a.ParseArgumentsAs<bool>())
                              .AddOption("--virtual-terminal-mode",
                                         "Shows output formatted using ANSI sequences",
                                         a => a.ParseArgumentsAs<bool>())
                              .AddVersionOption()


                              .OnExecute(typeof(CommandLine).GetMethod(nameof(InvokeAsync)));

            commandLine.Description = "A command line testing tool for .NET";

            Parser = commandLine.Build();
        }

        public static async Task<int> InvokeAsync(
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

            IConsoleView<TestResultSet> view = null;

            switch (format)
            {
                case OutputFormat.Summary:
                    if (!ansiMode)
                    {
                        view = new SummaryView(hideTestOutput);
                    }
                    else
                    {
                        view = new AnsiSummaryView(hideTestOutput);
                    }
                    break;
                case OutputFormat.Json:
                    view = new JsonView();
                    break;
            }

            await view.WriteAsync(console, resultSet);

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
