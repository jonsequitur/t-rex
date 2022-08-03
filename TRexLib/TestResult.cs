using System;
using System.IO;
using System.Linq;

namespace TRexLib;

[TypeFormatterSource(typeof(TypeFormatterSource))]
public class TestResult
{
    public TestResult(
        string fullyQualifiedTestName,
        TestOutcome outcome,
        TimeSpan? duration = null,
        DateTimeOffset? startTime = null,
        DateTimeOffset? endTime = null,
        DirectoryInfo testProjectDirectory = null,
        FileInfo testOutputFile = null,
        FileInfo codebase = null,
        string output = null,
        string stacktrace = null)
    {
        FullyQualifiedTestName = fullyQualifiedTestName;
        Duration = duration;
        StartTime = startTime;
        EndTime = endTime;
        Outcome = outcome;
        TestProjectDirectory = testProjectDirectory;
        TestOutputFile = testOutputFile;
        Codebase = codebase;
        Output = output;
        Stacktrace = stacktrace;

        var testNameParts = fullyQualifiedTestName.Split('.');

        if (testNameParts.Length > 1)
        {
            var testName = testNameParts[^1];
            var className = testNameParts[^2];
            var @namespace = string.Join(".", testNameParts.Take(testNameParts.Length - 2));

            // only infer these if fullyQualifiedTestName is typical of .NET tests
            if (!className.Contains(" ") &&
                !@namespace.Contains(" "))
            {
                TestName = testName;
                ClassName = className;
                Namespace = @namespace;
            }
        }

        if (TestName is null)
        {
            TestName = FullyQualifiedTestName;
        }
    }

    public string FullyQualifiedTestName { get; }
    public TimeSpan? Duration { get; }
    public DateTimeOffset? StartTime { get; }
    public DateTimeOffset? EndTime { get; }
    public TestOutcome Outcome { get; }
    public DirectoryInfo TestProjectDirectory { get; }
    public FileInfo TestOutputFile { get; }
    public FileInfo Codebase { get; }
    public string Output { get; }
    public string Stacktrace { get; }

    public string Namespace { get; set; }
    public string TestName { get; set; }
    public string ClassName { get; set; }

    public override string ToString()
    {
        var badge = Outcome switch
        {
            TestOutcome.Failed => "❌",
            TestOutcome.Passed => "✅",
            TestOutcome.NotExecuted => "⚠️",
            TestOutcome.Inconclusive => "⚠️",
            TestOutcome.Timeout => "⌚",
            TestOutcome.Pending => "⏳",
            _ => throw new ArgumentOutOfRangeException()
        };

        return $"{badge} {FullyQualifiedTestName}";
    }
}