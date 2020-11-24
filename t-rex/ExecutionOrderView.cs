using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.Linq;
using System.Threading.Tasks;
using TRexLib;

namespace TRex.CommandLine
{
    public class ExecutionOrderView : IConsoleView<TestResultSet>
    {
        private readonly OutputFormat outputFormat;

        public ExecutionOrderView(OutputFormat outputFormat)
        {
            this.outputFormat = outputFormat;
        }

        public Task WriteAsync(IConsole console, TestResultSet testResults)
        {
            var sorted = outputFormat switch
            {
                OutputFormat.OrderByStartTime => testResults.OrderBy(r => r.StartTime),

                OutputFormat.OrderByEndTime => testResults.OrderBy(r => r.EndTime),

                OutputFormat.OrderByDuration => testResults.OrderBy(r => r.Duration),

                _ => throw new ArgumentOutOfRangeException()
            };

            foreach (var result in sorted)
            {
                using var _ = console.SetColorForOutcome(result.Outcome);

                console.Out.WriteLine($"{result.StartTime:s}: {result.Namespace}.{result.ClassName}.{result.TestName}");
            }

            console.Out.WriteLine();

            View.WriteSummary(console, testResults);

            return Task.CompletedTask;
        }
    }
}