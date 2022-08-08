using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Recipes;
using static System.Environment;

namespace TRexLib;

public static class TestOutputDocumentParser
{
    public static TestResultSet Parse(this FileInfo fileInfo)
    {
        // assume .trx
        using var streamReader = fileInfo.OpenText();
        var xml = streamReader.ReadToEnd();

        return Parse(xml, fileInfo);
    }

    public static TestResultSet Parse(
        string xml,
        FileInfo testOutputFile = null)
    {
        XDocument document = null;
        TestResult[] elements;
        string testRunName = null;

        try
        {
            document = XDocument.Parse(xml);

            testRunName = document.Descendants()
                                  .Where(e => e.Name.LocalName == "TestRun")
                                  .Attributes("name")
                                  .Select(e => e.Value)
                                  .SingleOrDefault();

            var testDefinitions = document
                                  .Descendants()
                                  .Where(e => e.Name.LocalName == "TestDefinitions")
                                  .SelectMany(e => e.Descendants().Where(ee => ee.Name.LocalName == "UnitTest"))
                                  .ToDictionary(
                                      e => e.Elements().FirstOrDefault()?.Attribute("id")?.Value ?? Guid.NewGuid().ToString(),
                                      e => (
                                               codeBase: e.Elements().Skip(1).FirstOrDefault()?.Attribute("codeBase")?.Value,
                                               className: e.Elements().Skip(1).FirstOrDefault()?.Attribute("className")?.Value
                                           ));

            elements = document
                       .Descendants()
                       .Where(e => e.Name.LocalName == "UnitTestResult")
                       .Select(e =>
                       {
                           var stdOut = string.Join("\n", e.Descendants()
                                                           .Where(ee => ee.Name.LocalName == "StdOut")
                                                           .Select(ee => ee.Value));
                           
                           var errorMessage = string.Join("\n", e.Descendants()
                                                           .Where(ee => ee.Name.LocalName == "Message")
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

                           var codeBase = GetFromTestDefinition(
                               e,
                               t => !string.IsNullOrWhiteSpace(t.codeBase)
                                        ? new FileInfo(t.codeBase)
                                        : null);

                           var testName = e.Attribute("testName") is { } attr
                                              ? attr.Value
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
                               testProjectDirectory: codeBase?.Directory
                                                             .Parent
                                                             .Parent
                                                             .Parent
                                                             .EnsureTrailingSlash(),
                               testOutputFile: testOutputFile,
                               codebase: codeBase,
                               stdOut: stdOut,
                               errorMessage: errorMessage,
                               stackTrace: stacktrace);
                       })
                       .ToArray();

            T GetFromTestDefinition<T>(XElement e, Func<(string codeBase, string className), T> getValue)
            {
                if (testDefinitions.TryGetValue(e.Attribute("executionId").Value, out var tuple))
                {
                    return getValue(tuple);
                }
                else
                {
                    return default;
                }
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine($"An exception occurred while parsing {testOutputFile?.FullName}{NewLine}{NewLine}{exception}{NewLine}{NewLine}{document}");
            throw;
        }

        var resultSet = new TestResultSet(elements);

        if (testRunName is { })
        {
            resultSet.TestRunName = testRunName;
        }

        return resultSet;
    }
}