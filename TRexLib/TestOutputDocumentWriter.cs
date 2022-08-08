using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace TRexLib;

public class TestOutputDocumentWriter
{
    private readonly TextWriter _writer;

    public TestOutputDocumentWriter(TextWriter writer)
    {
        _writer = writer;
    }

    public void Write(TestResultSet results)
    {
        var xdoc = new XDocument();

        var testListId = "8c84fa94-04c1-424b-9868-57a2d4851a1d";
        var testRunId = Guid.NewGuid();

        var timesElement = new XElement(
            "Times",
            new XAttribute("creation", results.CreatedTime),
            new XAttribute("queuing", results.QueuedTime),
            new XAttribute("start", results.StartedTime),
            new XAttribute("finish", results.CompletedTime));

        var testSettingsElement = new XElement(
            "TestSettings",
            new XAttribute("name", "default"),
            new XAttribute("id", Guid.NewGuid().ToString()));

        var resultsElement = new XElement(
            "Results",
            results.Select(r =>
            {
                var unitTestResultElement = new XElement(
                    "UnitTestResult",
                    new XAttribute("executionId", GetExecutionId(r)),
                    new XAttribute("testId", GetTestId(r)),
                    new XAttribute("testName", r.FullyQualifiedTestName),
                    new XAttribute("computerName", r.ComputerName ?? Environment.MachineName),
                    new XAttribute("duration", (r.Duration ?? new TimeSpan()).ToString()),
                    new XAttribute("startTime", r.StartTime.ToString()),
                    new XAttribute("endTime", r.EndTime.ToString()),
                    new XAttribute("outcome", r.Outcome.ToString()),
                    new XAttribute("testType", "13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b"), // i.e. "unit test"... is there something more appropriate?
                    new XAttribute("testListId", testListId));

                if (r.StackTrace is not null || r.ErrorMessage is not null || r.StdOut is not null)
                {
                    var outputElement = new XElement("Output");

                    if (r.ErrorMessage is not null || r.StackTrace is not null)
                    {
                        var errorInfoElement = new XElement("ErrorInfo");

                        if (r.ErrorMessage is not null)
                        {
                            errorInfoElement.Add(new XElement("Message", r.ErrorMessage));
                        }

                        if (r.StackTrace is not null)
                        {
                            errorInfoElement.Add(new XElement("StackTrace", r.StackTrace));
                        }

                        outputElement.Add(errorInfoElement);
                    }

                    if (r.StdOut is not null)
                    {
                        outputElement.Add(new XElement("StdOut", r.StdOut));
                    }

                    unitTestResultElement.Add(outputElement);
                }

                return unitTestResultElement;
            }));

        var testDefsElement = new XElement(
            "TestDefinitions",
            results.Select(r => new XElement(
                               "UnitTest",
                               new XAttribute("name", r.FullyQualifiedTestName),
                               new XAttribute("storage", results.TestFilePath),
                               new XAttribute("id", GetTestId(r)),
                               new XElement("Execution",
                                            new XAttribute("id", GetExecutionId(r))),
                               new XElement(
                                   "TestMethod",
                                   new XAttribute("codeBase", GetCodebase(r)),
                                   new XAttribute("className", r.FullyQualifiedClassName ?? ""),
                                   new XAttribute("name", r.TestName)))));

        var testEntriesElement = new XElement(
            "TestEntries",
            results.Select(r => new XElement(
                               "TestEntry",
                               new XAttribute("testId", GetTestId(r)),
                               new XAttribute("executionId", GetExecutionId(r)),
                               new XAttribute("testListId", testListId))));

        var testListElement = new XElement(
            "TestLists",
            new XElement(
                "TestList",
                new XAttribute("name", "Results Not in a List"), new XAttribute("id", testListId)));

        var resultSummaryElement = new XElement(
            "ResultSummary",
            new XElement(
                "Counters",
                new XAttribute("total", results.Count),
                new XAttribute("executed", results.Count(r => r.Outcome is not TestOutcome.NotExecuted)),
                new XAttribute("passed", results.Passed.Count),
                new XAttribute("failed", results.Failed.Count),
                new XAttribute("timeout", results.Count(r => r.Outcome == TestOutcome.Timeout)),
                new XAttribute("inconclusive", results.Count(r => r.Outcome == TestOutcome.Inconclusive)),
                new XAttribute("notExecuted", results.Count(r => r.Outcome == TestOutcome.NotExecuted)),
                new XAttribute("completed", results.Count(r => r.Outcome is TestOutcome.Passed or TestOutcome.Failed)),
                new XAttribute("pending", results.Count(r => r.Outcome == TestOutcome.Pending)))
        );

        var testRunElement = new XElement(
            "TestRun",
            new XAttribute("id", testRunId),
            new XAttribute("name", results.TestRunName),
            new XAttribute("xmlns", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"),
            timesElement,
            testSettingsElement,
            resultsElement,
            testDefsElement,
            testEntriesElement,
            testListElement,
            resultSummaryElement);

        xdoc.Add(new XElement(testRunElement));

        xdoc.WriteTo(new XmlTextWriter(_writer)
        {
            Indentation = 2,
            Formatting = Formatting.Indented
        });

        string GetTestId(TestResult r)
        {
            return r.FullyQualifiedTestName.ToGuidV3().ToString();
        }

        string GetExecutionId(TestResult r)
        {
            return r.FullyQualifiedTestName.ToGuidV3().ToString();
        }

        string GetCodebase(TestResult r)
        {
            return r?.Codebase?.FullName ?? "";
        }
    }
}