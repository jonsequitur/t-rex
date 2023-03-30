using System;
using System.Collections.Generic;
using System.IO;
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

    public async Task WriteAsync(TextWriter console, TestResultSet testResults)
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
        TextWriter console,
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

            console.Write($"{groupingByOutcome.Outcome.ToString().ToUpper()}     ");

            console.WriteDuration(durationForOutcome);
            console.WriteLine();

            foreach (var groupingByNamespace in groupingByOutcome.Items)
            {
                var durationForNamespace =
                    TimeSpan.FromSeconds(
                        groupingByNamespace
                            .Items
                            .Sum(className => className.Items
                                                       .Sum(test => test.Duration?.TotalSeconds)) ?? 0);

                console.Write($"  {groupingByNamespace.Namespace}     ");

                console.WriteDuration(durationForNamespace);
                console.WriteLine();

                foreach (var groupingByClassName in groupingByNamespace.Items)
                {
                    var durationForClass =
                        TimeSpan.FromSeconds(groupingByClassName.Items
                                                                .Sum(className => className.Duration?.TotalSeconds) ?? 0);

                    console.Write($"    {groupingByClassName.ClassName}     ");

                    console.WriteDuration(durationForClass);
                    console.WriteLine();

                    foreach (var result in groupingByClassName.Items)
                    {
                        var durationForTest = result.Duration
                                                    .IfNotNull()
                                                    .Then(d => TimeSpan.FromSeconds(d.TotalSeconds))
                                                    .ElseDefault();

                        console.Write($"      {result.TestName}     ");

                        console.WriteDuration(durationForTest);
                        console.WriteLine();

                        if (!HideTestOutput &&
                            groupingByOutcome.Outcome == TestOutcome.Failed)
                        {
                            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                            {
                                using (console.SetColor(ConsoleColor.Gray))
                                {
                                    console.WriteLine($"        {result.ErrorMessage.Replace("\r\n", "\n").Replace("\n", "        \n")}");
                                }
                            }
                            
                            if (!string.IsNullOrWhiteSpace(result.StdOut))
                            {
                                using (console.SetColor(ConsoleColor.Gray))
                                {
                                    console.WriteLine($"        {result.StdOut.Replace("\r\n", "\n").Replace("\n", "        \n")}");
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(result.StackTrace))
                            {
                                using (console.SetColor(ConsoleColor.DarkGray))
                                {
                                    console.WriteLine($"        Stack trace:");
                                }

                                using (console.SetColor(ConsoleColor.Gray))
                                {
                                    console.WriteLine($"        {result.StackTrace.Replace("\r\n", "\n").Replace("\n", "          \n")}");
                                }
                            }
                        }
                    }
                }
            }

            console.WriteLine();
        }

        return Task.CompletedTask;
    }
}