using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Recipes;
using TRexLib;

namespace TRex.CommandLine
{
    public class SummaryView : IConsoleView<TestResultSet>
    {
        public bool HideTestOutput { get; }

        public SummaryView(bool hideTestOutput)
        {
            HideTestOutput = hideTestOutput;
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

            console.Out.WriteLine("SUMMARY:");

            using (console.SetColorForOutcome(TestOutcome.Passed))
            {
                console.Out.Write($"Passed: {testResults.Passed.Count}, ");
            }

            using (console.SetColorForOutcome(TestOutcome.Failed))
            {
                console.Out.Write($"Failed: {testResults.Failed.Count}, ");
            }

            using (console.SetColorForOutcome(TestOutcome.NotExecuted))
            {
                console.Out.WriteLine($"Not run: {testResults.NotExecuted.Count}");
            }
        }

        public Task WriteResults(
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

                    console.Out.Write($"{groupingByOutcome.Outcome.ToString().ToUpper()}     ");

                    WriteDuration(durationForOutcome);

                    foreach (var groupingByNamespace in groupingByOutcome.Items)
                    {
                        var durationForNamespace =
                            groupingByNamespace
                                .Items
                                .Sum(className => className.Items
                                                           .Sum(test => test.Duration?.TotalSeconds));

                        console.Out.Write($"  {groupingByNamespace.Namespace}     ");

                        WriteDuration(durationForNamespace);

                        foreach (var groupingByClassName in groupingByNamespace.Items)
                        {
                            var durationForClass =
                                groupingByClassName.Items
                                                   .Sum(className => className.Duration?.TotalSeconds);

                            console.Out.Write($"    {groupingByClassName.ClassName}     ");

                            WriteDuration(durationForClass);

                            foreach (var result in groupingByClassName.Items)
                            {
                                var durationForTest = result.Duration.IfNotNull().Then(d => d.TotalSeconds).ElseDefault();
                                console.Out.Write($"      {result.TestName}     ");

                                WriteDuration(durationForTest);

                                if (!HideTestOutput &&
                                    groupingByOutcome.Outcome == TestOutcome.Failed)
                                {
                                    if (!string.IsNullOrWhiteSpace(result.Output))
                                    {
                                        using (console.SetColor(System.ConsoleColor.Gray))
                                        {
                                            console.Out.WriteLine($"        {result.Output.Replace("\r\n", "\n").Replace("\n", "        \n")}");
                                        }
                                    }

                                    if (!string.IsNullOrWhiteSpace(result.Stacktrace))
                                    {
                                        using (console.SetColor(System.ConsoleColor.DarkGray))
                                        {
                                            console.Out.WriteLine($"        Stack trace:");
                                        }

                                        using (console.SetColor(System.ConsoleColor.Gray))
                                        {
                                            console.Out.WriteLine($"        {result.Stacktrace.Replace("\r\n", "\n").Replace("\n", "          \n")}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                console.Out.WriteLine();
            }

            return Task.CompletedTask;

            void WriteDuration(double? duration)
            {
                using (console.SetColor(System.ConsoleColor.Gray))
                {
                    console.Out.WriteLine($"({duration}s)");
                }
            }
        }
    }
}