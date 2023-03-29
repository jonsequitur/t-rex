using System;
using System.IO;
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

        public Task WriteAsync(TextWriter console, TestResultSet testResults)
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

                console.Write($"{result.StartTime:s}: {result.Namespace}.{result.ClassName}.{result.TestName} ({result.Outcome}: ");
                
                console.WriteDuration(result.Duration);

                console.WriteLine(")");
            }

            console.WriteLine();

            View.WriteSummary(console, testResults);

            return Task.CompletedTask;
        }
    }
}