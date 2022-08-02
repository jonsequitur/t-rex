using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace TRexLib;

public class TestOutputFileWriter
{
    private readonly TextWriter _writer;

    public TestOutputFileWriter(TextWriter writer)
    {
        _writer = writer;
    }

    public void Write(TestResultSet results)
    {
        var xdoc = new XDocument();

        string testListId = Guid.NewGuid().ToString("N");
        var testRunId = Guid.NewGuid();

        var resultsElement = new XElement(
            "Results",
            results.Select(r => new XElement("UnitTestResult",
                                             new XAttribute("executionId", GetExecutionId(r)),
                                             new XAttribute("testId", GetTestId(r)),
                                             new XAttribute("testName", r.FullyQualifiedTestName),
                                             new XAttribute("duration", (r.Duration ?? new TimeSpan()).ToString()),
                                             new XAttribute("startTime", r.StartTime.ToString()),
                                             new XAttribute("endTime", r.EndTime.ToString()),
                                             new XAttribute("outcome", r.Outcome.ToString()),
                                             new XAttribute("testListId", testListId))));

        var testDefsElement = new XElement(
            "TestDefinitions",
            results.Select(r => new XElement("UnitTest",
                                             new XAttribute("name", r.FullyQualifiedTestName),
                                             new XAttribute("id", GetTestId(r)),
                                             new XElement("Execution",
                                                          new XAttribute("id", GetExecutionId(r))),
                                             new XElement("TestMethod",
                                                          new XAttribute("codeBase", GetCodebase(r)),
                                                          new XAttribute("className", r.ClassName ?? ""),
                                                          new XAttribute("name", r.TestName)))));

        var testEntriesElement = new XElement(
            "TestEntries");

        var testListElement = new XElement("TestLists",
                                           new XElement("TestList",
                                                        new XAttribute("name", "All tests"), new XAttribute("id", testListId)));

        var resultSummaryElement = new XElement(
            "ResultSummary");

        var testRunElement = new XElement(
            "TestRun",
            new XAttribute("id", testRunId),
            new XElement("Times"),
            new XElement("TestSettings"),
            resultsElement,
            testDefsElement,
            testEntriesElement,
            testListElement,
            resultSummaryElement
        );

        xdoc.Add(new XElement(testRunElement));

        xdoc.WriteTo(new XmlTextWriter(_writer)
        {
            Indentation = 3,
            Formatting = Formatting.Indented
        });

        string GetTestId(TestResult r)
        {
            return r.FullyQualifiedTestName.ToGuidV3().ToString("N");
        }

        string GetExecutionId(TestResult r)
        {
            return r.FullyQualifiedTestName.ToGuidV3().ToString("N");
        }

        string GetCodebase(TestResult r)
        {
            return r?.Codebase?.FullName ?? "";
        }
    }
}