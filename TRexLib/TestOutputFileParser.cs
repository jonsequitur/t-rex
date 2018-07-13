using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Recipes;
using static System.Environment;

namespace TRexLib
{
    public static class TestOutputFileParser
    {
        public static TestResultSet Parse(
            this FileInfo fileInfo)
        {
            // assume .trx
            using (var streamReader = fileInfo.OpenText())
            {
                XDocument document = null;

                TestResult[] elements;
                try
                {
                    document = XDocument.Parse(streamReader.ReadToEnd());

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

                                   return new TestResult(
                                       fullyQualifiedTestName: e.Attribute("testName")?.Value,
                                       outcome: e.Attribute("outcome")
                                                 .IfNotNull()
                                                 .Then(a => a.Value
                                                             .IfNotNull()
                                                             .Then(v => (TestOutcome) Enum.Parse(
                                                                       typeof(TestOutcome), v)))
                                                 .Else(() => TestOutcome.NotExecuted),
                                       duration: e.Attribute("duration")
                                                  ?.Value
                                                  .IfNotNull()
                                                  .Then(TimeSpan.Parse)
                                                  .ElseDefault(),
                                       startTime: e.Attribute("startTime")
                                                   ?.Value
                                                   .IfNotNull()
                                                   .Then(DateTimeOffset.Parse)
                                                   .ElseDefault(),
                                       endTime: e.Attribute("endTime")
                                                 ?.Value
                                                 .IfNotNull()
                                                 .Then(DateTimeOffset.Parse)
                                                 .ElseDefault(),
                                       testProjectDirectory: codebase?.Directory
                                                                     .Parent
                                                                     .Parent
                                                                     .Parent
                                                                     .EnsureTrailingSlash(),
                                       testOutputFile: fileInfo,
                                       codebase: codebase,
                                       output: output,
                                       stacktrace: stacktrace);
                               })
                               .ToArray();
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"An exception occurred while parsing {fileInfo.FullName}:{NewLine}{NewLine}{exception}{NewLine}{NewLine}{document}");
                    throw;
                }

                return new TestResultSet(elements);
            }
        }

        private static FileInfo CodebaseFor(
            Dictionary<string, string> testDefinitions,
            XElement e) =>
            testDefinitions.TryGetValue(e.Attribute("executionId").Value, out var codebase)
                ? new FileInfo(codebase)
                : null;
    }
}
