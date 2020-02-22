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
                kernelBase.AddDirective(TestCommand());
            }

            return Task.CompletedTask;
        }

        private static Command TestCommand()
        {
            var testCommand = new Command("#!test", "Runs unit tests using dotnet test")
            {
                new Argument<FileSystemInfo>("project", getDefaultValue: () => new DirectoryInfo(Directory.GetCurrentDirectory()))
                {
                    Description = "The project or solution file to operate on. If a file is not specified, the command will search the current directory for one."
                }.ExistingOnly()
            };

            testCommand.Handler = CommandHandler.Create<FileSystemInfo, KernelInvocationContext>(async (project, context) =>
            {
                var dotnet = new Dotnet();

                var result = await dotnet.Execute($"test -l:trx \"{project.FullName}\"");

                result.ThrowOnFailure();

                var dir = project switch
                {
                    DirectoryInfo directoryInfo => directoryInfo,
                    FileInfo fileInfo => fileInfo.Directory,
                    _ => throw new ArgumentOutOfRangeException(nameof(project))
                };

                var trxFiles = TestResultSet.FindTrxFiles(dir.FullName);

                var results = TestResultSet.Create(trxFiles);

                await context.DisplayAsync(results);
            });

            return testCommand;
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
                    table(
                        thead(
                            tr(
                                th("Test"),
                                th("Result"),
                                th("Duration"))),
                        tbody(
                            set.OrderBy(r => r.Outcome switch
                               {
                                   TestOutcome.Failed => 0,
                                   TestOutcome.Passed => 1,
                                   _ => 2
                               })
                               .ThenBy(r => r.FullyQualifiedTestName)
                               .Select(result =>
                                           tr[style: OutcomeStyle(result.Outcome)](
                                               td[style: "text-align:left"](result.FullyQualifiedTestName),
                                               td(result.Outcome.ToString()),
                                               td(result.Duration?.TotalSeconds + "s"))))));

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