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
        TestName = testNameParts[^1];
        if (testNameParts.Length > 1)
        {
            ClassName = testNameParts[^2];
            Namespace = string.Join(".", testNameParts.Take(testNameParts.Length - 2));
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

    public string Namespace { get; }
    public string TestName { get; }
    public string ClassName { get; }

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