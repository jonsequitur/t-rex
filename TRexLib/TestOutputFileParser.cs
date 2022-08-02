using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Recipes;
using static System.Environment;

namespace TRexLib;

public static class TestOutputFileParser
{
    public static TestResultSet Parse(this FileInfo fileInfo)
    {
        // assume .trx
        using var streamReader = fileInfo.OpenText();
        var xml = streamReader.ReadToEnd();

        return Parse(xml, fileInfo);
    }

    public static TestResultSet Parse(string xml, FileInfo testOutputFile = null)
    {
        XDocument document = null;
        TestResult[] elements;
        try
        {
            document = XDocument.Parse(xml);

            var testResultElements = document
                                     .Descendants()
                                     .Where(e => e.Name.LocalName == "UnitTestResult");

            var testDefinitions = document
                                  .Descendants()
                                  .Where(e => e.Name.LocalName == "TestDefinitions")
                                  .SelectMany(e => e.Descendants().Where(ee => ee.Name.LocalName == "UnitTest"))
                                  .ToDictionary(
                                      e => e.Elements().FirstOrDefault()?.Attribute("id")?.Value ?? Guid.NewGuid().ToString(),
                                      e => e.Elements().Skip(1).FirstOrDefault()?.Attribute("codeBase")?.Value);

            elements = testResultElements
                       .Select(e =>
                       {
                           var codebase = CodebaseFor(testDefinitions, e);

                           var output = string.Join("\n", e.Descendants()
                                                           .Where(ee => ee.Name.LocalName == "Message" ||
                                                                        ee.Name.LocalName == "StdOut")
                                                           .Select(ee => ee.Value));

                           var stacktrace = string.Join("\n", e.Descendants()
                                                               .Where(ee => ee.Name.LocalName == "StackTrace")
                                                               .Select(ee => ee.Value));

                           var startTime = e.Attribute("startTime") is { } startTimeString &&
                                           !string.IsNullOrWhiteSpace(startTimeString.Value)
                                               ? DateTime.Parse(startTimeString.Value)
                                               : default;

                           var endTime = e.Attribute("endTime") is { } endTimeString &&
                                         !string.IsNullOrWhiteSpace(endTimeString.Value)
                                             ? DateTime.Parse(endTimeString.Value)
                                             : default;

                           var duration = e.Attribute("duration") is { } durationString &&
                                          !string.IsNullOrWhiteSpace(durationString.Value)
                                              ? TimeSpan.Parse(durationString.Value)
                                              : default;

                           return new TestResult(
                               fullyQualifiedTestName: e.Attribute("testName")?.Value,
                               outcome: e.Attribute("outcome")
                                         .IfNotNull()
                                         .Then(a => a.Value
                                                     .IfNotNull()
                                                     .Then(v => (TestOutcome)Enum.Parse(
                                                               typeof(TestOutcome), v)))
                                         .Else(() => TestOutcome.NotExecuted),
                               duration: duration,
                               startTime: startTime,
                               endTime: endTime,
                               testProjectDirectory: codebase?.Directory
                                                             .Parent
                                                             .Parent
                                                             .Parent
                                                             .EnsureTrailingSlash(),
                               testOutputFile: testOutputFile,
                               codebase: codebase,
                               output: output,
                               stacktrace: stacktrace);
                       })
                       .ToArray();
        }
        catch (Exception exception)
        {
            Console.WriteLine($"An exception occurred while parsing {testOutputFile?.FullName}{NewLine}{NewLine}{exception}{NewLine}{NewLine}{document}");
            throw;
        }

        return new TestResultSet(elements);
    }

    private static FileInfo CodebaseFor(
        Dictionary<string, string> testDefinitions,
        XElement e)
    {
        return testDefinitions.TryGetValue(e.Attribute("executionId").Value, out var codebase) &&
               !string.IsNullOrEmpty(codebase)
                   ? new FileInfo(codebase)
                   : null;
    }
}