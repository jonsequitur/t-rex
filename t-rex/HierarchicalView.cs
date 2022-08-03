using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Recipes;
using TRexLib;

namespace TRex.CommandLine;

public class HierarchicalView : IConsoleView<TestResultSet>
{
    public bool HideTestOutput { get; }

    public HierarchicalView(bool hideTestOutput)
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

        View.WriteSummary(console, testResults);
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
            using var _ = console.SetColorForOutcome(groupingByOutcome.Outcome);

            var durationForOutcome =
                TimeSpan.FromSeconds(
                    groupingByOutcome
                        .Items
                        .Sum(ns =>
                                 ns.Items
                                   .Sum(className =>
                                            className.Items
                                                     .Sum(test => test.Duration?.TotalSeconds))) ?? 0);

            console.Out.Write($"{groupingByOutcome.Outcome.ToString().ToUpper()}     ");

            console.WriteDuration(durationForOutcome);
            console.Out.WriteLine();

            foreach (var groupingByNamespace in groupingByOutcome.Items)
            {
                var durationForNamespace =
                    TimeSpan.FromSeconds(
                        groupingByNamespace
                            .Items
                            .Sum(className => className.Items
                                                       .Sum(test => test.Duration?.TotalSeconds)) ?? 0);

                console.Out.Write($"  {groupingByNamespace.Namespace}     ");

                console.WriteDuration(durationForNamespace);
                console.Out.WriteLine();

                foreach (var groupingByClassName in groupingByNamespace.Items)
                {
                    var durationForClass =
                        TimeSpan.FromSeconds(groupingByClassName.Items
                                                                .Sum(className => className.Duration?.TotalSeconds) ?? 0);

                    console.Out.Write($"    {groupingByClassName.ClassName}     ");

                    console.WriteDuration(durationForClass);
                    console.Out.WriteLine();

                    foreach (var result in groupingByClassName.Items)
                    {
                        var durationForTest = result.Duration
                                                    .IfNotNull()
                                                    .Then(d => TimeSpan.FromSeconds(d.TotalSeconds))
                                                    .ElseDefault();

                        console.Out.Write($"      {result.TestName}     ");

                        console.WriteDuration(durationForTest);
                        console.Out.WriteLine();

                        if (!HideTestOutput &&
                            groupingByOutcome.Outcome == TestOutcome.Failed)
                        {
                            if (!string.IsNullOrWhiteSpace(result.Output))
                            {
                                using (console.SetColor(ConsoleColor.Gray))
                                {
                                    console.Out.WriteLine($"        {result.Output.Replace("\r\n", "\n").Replace("\n", "        \n")}");
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(result.Stacktrace))
                            {
                                using (console.SetColor(ConsoleColor.DarkGray))
                                {
                                    console.Out.WriteLine($"        Stack trace:");
                                }

                                using (console.SetColor(ConsoleColor.Gray))
                                {
                                    console.Out.WriteLine($"        {result.Stacktrace.Replace("\r\n", "\n").Replace("\n", "          \n")}");
                                }
                            }
                        }
                    }
                }
            }

            console.Out.WriteLine();
        }

        return Task.CompletedTask;
    }
}