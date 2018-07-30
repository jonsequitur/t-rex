using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
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
                              .UseHelp()
                              .UseSuggestDirective()
                              .UseParseErrorReporting()
                              .UseExceptionHandler()
                              .AddOption("--file",
                                         ".trx file(s) to parse",
                                         a => a.ExistingFilesOnly()
                                               .ParseArgumentsAs<FileInfo[]>())
                              .AddOption("--filter",
                                         "Only look at tests matching the filter. \"*\" can be used as a wildcard.",
                                         args => args.ExactlyOne())
                              .AddOption("--format",
                                         "The format for the output.",
                                         args => args.WithDefaultValue(() => OutputFormat.Summary)
                                                     .ParseArgumentsAs<OutputFormat>())
                              .AddOption("--path",
                                         "Directory or directories to search for .trx files. Only the most recent .trx file in a given directory is used.",
                                         a => a.WithDefaultValue(Directory.GetCurrentDirectory)
                                               .ParseArgumentsAs<DirectoryInfo[]>())
                              .AddOption("--show-test-output",
                                         "For failed tests, display the output.",
                                         a => a.ParseArgumentsAs<bool>())
                              .AddVersionOption()
                              .OnExecute(typeof(CommandLine).GetMethod(nameof(InvokeAsync)));

            commandLine.Description = "A command line testing tool for .NET";

            Parser = commandLine.Build();
        }

        public static async Task InvokeAsync(
            OutputFormat format,
            FileInfo[] file,
            DirectoryInfo[] path,
            string filter,
            bool showTestOutput,
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
                        allFiles.AddRange(SearchDirectory(directoryInfo.FullName));
                    }
                }
            }

            var resultSet = Create(allFiles);

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var regex = new Regex($"^{filter.Replace("*", ".*")}$", RegexOptions.IgnoreCase);

                resultSet = new TestResultSet(
                    resultSet.Where(r => regex.IsMatch(r.FullyQualifiedTestName)));
            }

            IConsoleView<TestResultSet> view = null;

            switch (format)
            {
                case OutputFormat.Summary:
                    view = new SummaryView(showTestOutput);
                    break;
                case OutputFormat.Json:
                    view = new JsonView();
                    break;
            }

            await view.WriteAsync(console, resultSet);
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
