using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Recipes;
using TRexLib;

namespace TRex.CommandLine
{
    public class SummaryWriter : IConsoleWriter
    {
        public bool ShowTestOutput { get; }

        public SummaryWriter(bool showTestOutput)
        {
            ShowTestOutput = showTestOutput;
        }

        public async Task WriteAsync(IConsole console, TestResultSet testResults)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            if (testResults == null)
            {
                throw new ArgumentNullException(nameof(testResults));
            }

            await WriteResults(console, testResults);

            console.Out.WriteLine($"SUMMARY:");

            using (console.SetColorForOutcome(TestOutcome.Passed))
            {
                await console.Out.WriteAsync($"Passed: {testResults.Passed.Count}, ");
            }

            using (console.SetColorForOutcome(TestOutcome.Failed))
            {
                await console.Out.WriteAsync($"Failed: {testResults.Failed.Count}, ");
            }

            using (console.SetColorForOutcome(TestOutcome.NotExecuted))
            {
                await console.Out.WriteLineAsync($"Not run: {testResults.NotExecuted.Count}");
            }
        }

        public async Task WriteResults(
            IConsole console,
            IEnumerable<TestResult> results)
        {
            var groupings = results
                            .GroupBy(result => result.Outcome)
                            .Select(
                                byOutcome => new
                                {
                                    Outcome = byOutcome.Key,
                                    Items = byOutcome
                                            .GroupBy(result => result.Namespace)
                                            .Select(
                                                byNamespace =>
                                                    new
                                                    {
                                                        Namespace = byNamespace.Key,
                                                        Items = byNamespace
                                                                .GroupBy(result => result.ClassName)
                                                                .Select(
                                                                    byClass => new
                                                                    {
                                                                        ClassName = byClass.Key,
                                                                        Items = byClass.Select(g => g)
                                                                    })
                                                    })
                                });

            foreach (var groupingByOutcome in groupings.OrderBy(t => t.Outcome))
            {
                using (console.SetColorForOutcome(groupingByOutcome.Outcome))
                {
                    var durationForOutcome =
                        groupingByOutcome
                            .Items
                            .Sum(ns =>
                                     ns.Items
                                       .Sum(className =>
                                                className.Items
                                                         .Sum(test => test.Duration?.TotalSeconds)));

                    await console.Out.WriteAsync($"{groupingByOutcome.Outcome.ToString().ToUpper()}     ");

                    await WriteDuration(durationForOutcome);

                    foreach (var groupingByNamespace in groupingByOutcome.Items)
                    {
                        var durationForNamespace =
                            groupingByNamespace
                                .Items
                                .Sum(className => className.Items
                                                           .Sum(test => test.Duration?.TotalSeconds));

                        await console.Out.WriteAsync($"  {groupingByNamespace.Namespace}     ");

                        await WriteDuration(durationForNamespace);

                        foreach (var groupingByClassName in groupingByNamespace.Items)
                        {
                            var durationForClass =
                                groupingByClassName.Items
                                                   .Sum(className => className.Duration?.TotalSeconds);

                            await console.Out.WriteAsync($"    {groupingByClassName.ClassName}     ");

                            await WriteDuration(durationForClass);

                            foreach (var result in groupingByClassName.Items)
                            {
                                var durationForTest = result.Duration.IfNotNull().Then(d => d.TotalSeconds).ElseDefault();
                                await console.Out.WriteAsync($"      {result.TestName}     ");

                                await WriteDuration(durationForTest);
                            }

                            if (ShowTestOutput &&
                                groupingByOutcome.Outcome == TestOutcome.Failed)
                            {
                                foreach (var result in groupingByClassName.Items)
                                {
                                    if (!string.IsNullOrWhiteSpace(result.Output))
                                    {
                                        using (console.SetColor(System.ConsoleColor.Gray))
                                        {
                                            await console.Out.WriteLineAsync($"        {result.Output.Replace("\r\n", "\n").Replace("\n", "        ")}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                await console.Out.WriteLineAsync();
            }

            async Task WriteDuration(double? duration)
            {
                using (console.SetColor(System.ConsoleColor.Gray))
                {
                    await console.Out.WriteLineAsync($"({duration}s)");
                }
            }
        }
    }
}
