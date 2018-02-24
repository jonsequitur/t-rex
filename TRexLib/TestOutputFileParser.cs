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

                                   return new TestResult
                                   {
                                       TestName = e.Attribute("testName")?.Value,
                                       Duration = e.Attribute("duration")
                                                   ?.Value
                                                   .IfNotNull()
                                                   .Then(TimeSpan.Parse)
                                                   .ElseDefault(),
                                       StartTime = e.Attribute("startTime")
                                                    ?.Value
                                                    .IfNotNull()
                                                    .Then(DateTimeOffset.Parse)
                                                    .ElseDefault(),
                                       EndTime = e.Attribute("endTime")
                                                  ?.Value
                                                  .IfNotNull()
                                                  .Then(DateTimeOffset.Parse)
                                                  .ElseDefault(),
                                       Outcome = e.Attribute("outcome")
                                                  .IfNotNull()
                                                  .Then(a => a.Value
                                                              .IfNotNull()
                                                              .Then(v => (TestOutcome) Enum.Parse(
                                                                        typeof(TestOutcome), v)))
                                                  .Else(() => TestOutcome.NotExecuted),
                                       TestOutputFile = fileInfo,
                                       Codebase = codebase,
                                       TestProjectDirectory = codebase?.Directory
                                                                      .Parent
                                                                      .Parent
                                                                      .Parent
                                                                      .EnsureTrailingSlash()
                                   };
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
