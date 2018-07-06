using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TRexLib;

namespace TRex.CommandLine
{
    public class CommandLine
    {
        public static Parser Parser { get; }

        private readonly string[] args;

        public CommandLine(string args) :
            this(args.Split(
                     new[] { ' ' },
                     StringSplitOptions.RemoveEmptyEntries))
        {
        }

        public CommandLine(string[] args)
        {
            this.args = args;
        }

        static CommandLine()
        {
            var builder = new CommandLineBuilder()
                          .UseParseDirective()
                          .UseHelp()
                          .UseSuggestDirective()
                          .UseParseErrorReporting()
                          .UseExceptionHandler()
                          .AddOption("--path",
                                     "Directories to search for .trx files. Only the most recent .trx file in a given directory is used.",
                                     a => a.WithDefaultValue(Directory.GetCurrentDirectory)
                                           .ParseArgumentsAs<DirectoryInfo[]>())
                          .AddOption("--file",
                                     ".trx files to parse",
                                     a => a
                                          .ExistingFilesOnly()
                                          .ParseArgumentsAs<FileInfo[]>())
                          .AddOption(new[] { "-f", "--format" },
                                     "The format for the output.",
                                     args => args
                                             .WithDefaultValue(() => OutputFormat.Summary)
                                             .ParseArgumentsAs<OutputFormat>());

            builder.OnExecute(typeof(CommandLine).GetMethod(nameof(InvokeAsync)));

            Parser = builder.Build();
        }

        public static async Task InvokeAsync(
            OutputFormat format,
            FileInfo[] file,
            DirectoryInfo[] path,
            IConsole console)
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

            IConsoleWriter writer = null;

            switch (format)
            {
                case OutputFormat.Summary:
                    writer = new SummaryWriter();
                    break;
                case OutputFormat.Json:
                    writer = new JsonWriter();
                    break;
            }

            await writer.WriteAsync(console, resultSet);
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
