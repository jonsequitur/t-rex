using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
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
            var fileOption = new Option<FileInfo[]>(
                    "--file", ".trx file(s) to parse")
                .ExistingOnly();

            var filterOption = new Option<string>(
                new[] { "-f", "--filter" },
                "Only look at tests containing the specified text. \"*\" can be used as a wildcard.");

            var formatOption = new Option<OutputFormat>(
                "--format",
                description: "The format for the output.",
                getDefaultValue: () => OutputFormat.Hierarchical);

            var pathOption = new Option<DirectoryInfo[]>(
                "--path",
                description:
                "Directory or directories to search for .trx files. Only the most recent .trx file in a given directory is used.",
                getDefaultValue: () => new[] { new DirectoryInfo(Directory.GetCurrentDirectory()) });

            var hideTestOutputOption = new Option<bool>(
                new[]
                {
                    "-d",
                    "--hide-test-output"
                },
                "For failed tests, hide detailed test output.");

            var allOption = new Option<bool>(
                "--all",
                "Show tests results for all test runs. By default, only the latest test run is shown.");

            var rootCommand = new RootCommand("A command line testing tool for .NET")
            {
                fileOption,
                filterOption,
                formatOption,
                pathOption,
                hideTestOutputOption,
                allOption,
            };

            rootCommand.SetHandler(
                Run,
                formatOption,
                fileOption,
                pathOption,
                filterOption,
                hideTestOutputOption,
                allOption,
                Bind.FromServiceProvider<IConsole>());

            Parser = new CommandLineBuilder(rootCommand)
                     .UseDefaults()
                     .Build();

            async Task<int> Run(
                OutputFormat format,
                FileInfo[] file,
                DirectoryInfo[] path,
                string filter,
                bool hideTestOutput,
                bool showAllResults,
                IConsole console)
            {
                return await DisplayResults(format, file, path, filter, hideTestOutput, showAllResults, console);
            }
        }

        public static async Task<int> DisplayResults(
            OutputFormat format,
            FileInfo[] file,
            DirectoryInfo[] path,
            string filter,
            bool hideTestOutput,
            bool showAllResults,
            IConsole console)
        {
            var allFiles = new List<FileInfo>();

            if (file is { } && file.Any())
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
                        allFiles.AddRange(TestResultSet.FindTrxFiles(directoryInfo.FullName, !showAllResults));
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
                case OutputFormat.Hierarchical:
                {
                    var view = new HierarchicalView(hideTestOutput);
                    await view.WriteAsync(console, resultSet);

                    break;
                }

                case OutputFormat.Json:
                {
                    var view = new JsonView();
                    await view.WriteAsync(console, resultSet);

                    break;
                }

                default:
                {
                    var view = new ExecutionOrderView(format);
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