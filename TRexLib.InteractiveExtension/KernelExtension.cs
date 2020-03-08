using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Formatting;
using Microsoft.DotNet.Interactive.Utility;
using XPlot.Plotly;
using static Microsoft.DotNet.Interactive.Formatting.PocketViewTags;

namespace TRexLib.InteractiveExtension
{
    public class KernelExtension : IKernelExtension
    {
        public Task OnLoadAsync(IKernel kernel)
        {
            RegisterFormatters();

            if (kernel is KernelBase kernelBase)
            {
                var trex = new Command("#!t-rex", "Run unit tests from a notebook.")
                {
                    RunCommand(),
                    ShowCommand()
                };
                kernelBase.AddDirective(trex);
            }

            return Task.CompletedTask;
        }

        private static Command RunCommand()
        {
            var runTestsCommand = new Command("run", "Runs unit tests using dotnet test")
            {
                new Argument<FileSystemInfo>("project", getDefaultValue: () => new DirectoryInfo(Directory.GetCurrentDirectory()))
                {
                    Description = "The project or solution file to operate on. If a file is not specified, the command will search the current directory for one."
                }.ExistingOnly()
            };

            runTestsCommand.Handler = CommandHandler.Create<FileSystemInfo, KernelInvocationContext>(RunTests);

            return runTestsCommand;
        }

        private static Command ShowCommand()
        {
            var showTestsCommand = new Command("show", "Show the results of the latest test run")
            {
                new Argument<DirectoryInfo>("dir", getDefaultValue: () => new DirectoryInfo(Directory.GetCurrentDirectory()))
                {
                    Description = "The directory under which to search for .trx files"
                }.ExistingOnly()
            };

            showTestsCommand.Handler = CommandHandler.Create<DirectoryInfo, KernelInvocationContext>(ShowTests);

            return showTestsCommand;
        }

        private static async Task RunTests(FileSystemInfo project, KernelInvocationContext context)
        {
            var dotnet = new Dotnet();

            var result = await dotnet.StartProcess(
                                         $"test -l:trx \"{project.FullName}\"",
                                         output => context.DisplayStandardOut(output + "\n"),
                                         error => context.DisplayStandardError(error + "\n"))
                                     .CompleteAsync();

            var dir = project switch
            {
                DirectoryInfo directoryInfo => directoryInfo,
                FileInfo fileInfo => fileInfo.Directory,
                _ => throw new ArgumentOutOfRangeException(nameof(project))
            };

            await ShowTests(dir, context);
        }

        private static async Task ShowTests(DirectoryInfo dir, KernelInvocationContext context)
        {
            var trxFiles = TestResultSet.FindTrxFiles(dir.FullName);

            var results = TestResultSet.Create(trxFiles);

            await context.DisplayAsync(results);
        }

        private void RegisterFormatters()
        {
            Formatter<TestResultSet>.Register((set, writer) =>
            {
                var pieChart = new Graph.Pie
                {
                    values = new[]
                    {
                        set.Count(r => r.Outcome == TestOutcome.Passed),
                        set.Count(r => r.Outcome == TestOutcome.Failed),
                        set.Count(r => r.Outcome == TestOutcome.NotExecuted),
                    },
                    labels = new[]
                    {
                        "Passed",
                        "Failed",
                        "Skipped"
                    },
                    marker = new Graph.Marker
                    {
                        colors = new[]
                        {
                            "green",
                            "red",
                            "#9b870c"
                        }
                    },
                    textinfo = "value",
                    hole = 0.4
                };

                var chart = Chart.Plot(pieChart);

                IHtmlContent view = div(
                    div(chart.GetHtmlContent()),
                    table[style: "width=100%"](
                        thead(
                            tr(
                                th[style: "text-align:left;width=70%"]("Test"),
                                th[style: "width=15%"]("Result"),
                                th[style: "width=15%"]("Duration"))),
                        tbody(
                            set.OrderBy(r => r.Outcome switch
                               {
                                   TestOutcome.Failed => 0,
                                   TestOutcome.Passed => 1,
                                   _ => 2
                               })
                               .ThenBy(r => r.FullyQualifiedTestName)
                               .Select(result =>
                               {
                                   // allow line breaks at periods if wrapping is needed
                                   var testName = Kernel.HTML(result.FullyQualifiedTestName.Replace(".", "&#8203."));

                                   var content = 
                                       result.Outcome == TestOutcome.Failed
                                       ? div(testName, br, pre[style:"padding-left:2em"](result.Output))
                                       : testName;

                                   return tr[style: OutcomeStyle(result.Outcome)](
                                       td[style: "text-align:left;width=75%"](
                                           content),
                                       td[style: "width=15%"](
                                           result.Outcome.ToString()),
                                       td[style: "width=15%"](
                                           result.Duration?.TotalSeconds + "s"));
                               }))));

                view.WriteTo(writer, HtmlEncoder.Default);
            }, HtmlFormatter.MimeType);
        }

        private string OutcomeStyle(TestOutcome outcome)
        {
            var color = outcome switch
            {
                TestOutcome.Failed => "red",
                TestOutcome.Passed => "green",
                _ => "#9b870c",
            };

            return $"color:{color}";
        }
    }
}