using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TRexLib;

namespace TRex.CommandLine
{
    public static class CommandLine
    {
        static CommandLine()
        {
            var fileOption = new Option<FileInfo[]>(
                    "--file") { Description = ".trx file(s) to parse" }
                .AcceptExistingOnly();

            var filterOption = new Option<string>(
                "--filter",
                "-f")
                { Description = "Only look at tests containing the specified text. \"*\" can be used as a wildcard." };

            var formatOption = new Option<OutputFormat>("--format")
            {
                Description = "The format for the output.",
                DefaultValueFactory = _ => OutputFormat.Hierarchical
            };

            var pathOption = new Option<DirectoryInfo[]>("--path")
            {
                Description = "Directory or directories to search for .trx files. Only the most recent .trx file in a given directory is used.",
                DefaultValueFactory = _ => new[] { new DirectoryInfo(Directory.GetCurrentDirectory()) }
            };

            var hideTestOutputOption = new Option<bool>("--hide-test-output", "-d")
            {
                Description = "For failed tests, hide detailed test output."
            };

            var allOption = new Option<bool>(
                    "--all")
                { Description = "Show tests results for all test runs. By default, only the latest test run is shown." };

            var rootCommand = new RootCommand("A command line testing tool for .NET")
            {
                fileOption,
                filterOption,
                formatOption,
                pathOption,
                hideTestOutputOption,
                allOption,
            };

            rootCommand.SetAction(Run);

            RootCommand = rootCommand;

            async Task<int> Run(ParseResult parseResult, CancellationToken token)
            {
                OutputFormat format = parseResult.GetValue(formatOption);
                FileInfo[] file = parseResult.GetValue(fileOption);
                DirectoryInfo[] path = parseResult.GetValue(pathOption);
                string filter = parseResult.GetValue(filterOption);
                bool hideTestOutput = parseResult.GetValue(hideTestOutputOption);
                bool showAllResults = parseResult.GetValue(allOption);

                return await DisplayResults(parseResult.InvocationConfiguration.Output, format, file, path, filter, hideTestOutput, showAllResults);
            }
        }

        public static RootCommand RootCommand { get; private set; }

        public static async Task<int> DisplayResults(
            TextWriter output,
            OutputFormat format,
            FileInfo[] file,
            DirectoryInfo[] path,
            string filter,
            bool hideTestOutput,
            bool showAllResults)
        {
            var allFiles = new List<FileInfo>();

            if (file is not null && file.Any())
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
                    await view.WriteAsync(output, resultSet);

                    break;
                }

                case OutputFormat.Json:
                {
                    var view = new JsonView();
                    await view.WriteAsync(output, resultSet);

                    break;
                }

                default:
                {
                    var view = new ExecutionOrderView(format);
                    await view.WriteAsync(output, resultSet);

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