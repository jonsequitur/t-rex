using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Rendering;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Recipes;
using Pocket;
using TRexLib;

namespace TRex.CommandLine
{
    public class AnsiSummaryView : IConsoleView<TestResultSet>
    {
        private readonly List<Span> _spans = new List<Span>();

        public bool HideTestOutput { get; }

        public AnsiSummaryView(bool hideTestOutput)
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

            _spans.Add(Format($"SUMMARY:"));

            using (SetColorForOutcome(TestOutcome.Passed))
            {
                _spans.Add(Format($"Passed: {testResults.Passed.Count}, "));
            }

            using (SetColorForOutcome(TestOutcome.Failed))
            {
                _spans.Add(Format($"Failed: {testResults.Failed.Count}, "));
            }

            using (SetColorForOutcome(TestOutcome.NotExecuted))
            {
                _spans.Add(Format($"Not run: {testResults.NotExecuted.Count}"));
            }
        }

        private IDisposable SetColorForOutcome(TestOutcome outcome)
        {
            ForegroundColorSpan color;

            switch (outcome)
            {
                case TestOutcome.Passed:
                    color = ForegroundColorSpan.Green;
                    break;
                case TestOutcome.Failed:
                    color = ForegroundColorSpan.Red;
                    break;
                case TestOutcome.NotExecuted:
                case TestOutcome.Inconclusive:
                case TestOutcome.Timeout:
                case TestOutcome.Pending:
                    color = ForegroundColorSpan.Yellow;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null);
            }

            _spans.Add(color);
            return Disposable.Create(() => _spans.Add(ForegroundColorSpan.Reset));
        }

        private readonly SpanFormatter _spanFormatter = new SpanFormatter();

        private Span Format(FormattableString value)
        {
            return _spanFormatter.Format(value);
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
                using (SetColorForOutcome(groupingByOutcome.Outcome))
                {
                    var durationForOutcome =
                        groupingByOutcome
                            .Items
                            .Sum(ns =>
                                     ns.Items
                                       .Sum(className =>
                                                className.Items
                                                         .Sum(test => test.Duration?.TotalSeconds)));

                    _spans.Add(Format($"{groupingByOutcome.Outcome.ToString().ToUpper()}     "));

                    WriteDuration(durationForOutcome);

                    foreach (var groupingByNamespace in groupingByOutcome.Items)
                    {
                        var durationForNamespace =
                            groupingByNamespace
                                .Items
                                .Sum(className => className.Items
                                                           .Sum(test => test.Duration?.TotalSeconds));

                        _spans.Add(Format($"  {groupingByNamespace.Namespace}     "));

                        WriteDuration(durationForNamespace);

                        foreach (var groupingByClassName in groupingByNamespace.Items)
                        {
                            var durationForClass =
                                groupingByClassName.Items
                                                   .Sum(className => className.Duration?.TotalSeconds);

                            _spans.Add(Format($"    {groupingByClassName.ClassName}     "));

                            WriteDuration(durationForClass);

                            foreach (var result in groupingByClassName.Items)
                            {
                                var durationForTest = result.Duration.IfNotNull().Then(d => d.TotalSeconds).ElseDefault();
                                _spans.Add(Format($"      {result.TestName}     "));

                                WriteDuration(durationForTest);

                                if (!HideTestOutput &&
                                    groupingByOutcome.Outcome == TestOutcome.Failed)
                                {
                                    if (!string.IsNullOrWhiteSpace(result.Output))
                                    {
                                        _spans.Add(
                                            Format(
                                                $"{ForegroundColorSpan.DarkGray}        {result.Output.Replace("\r\n", "\n").Replace("\n", "        \n")}{ForegroundColorSpan.Reset}"));
                                    }

                                    if (!string.IsNullOrWhiteSpace(result.Stacktrace))
                                    {
                                        _spans.Add(
                                            Format(
                                                $"{ForegroundColorSpan.DarkGray}        Stack trace:"));
                                        _spans.Add(
                                            Format(
                                                $"{ForegroundColorSpan.DarkGray}        {result.Stacktrace.Replace("\r\n", "\n").Replace("\n", "          \n")}{ForegroundColorSpan.Reset}"));
                                    }
                                }
                            }
                        }
                    }
                }

                var renderer = new ConsoleRenderer(console, OutputMode.Ansi);
                renderer.RenderToRegion(
                    new ContainerSpan(_spans.ToArray()),
                    new Region(0, Console.CursorTop, int.MaxValue, int.MaxValue, false));
            }

            void WriteDuration(double? duration) => _spans.Add(Format($"{ForegroundColorSpan.DarkGray}({duration}s){ForegroundColorSpan.Reset}{Environment.NewLine}"));
        }
    }
}
