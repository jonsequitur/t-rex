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
        string stackTrace = null,
        string errorMessage = null,
        string stdOut = null)
    {
        FullyQualifiedTestName = fullyQualifiedTestName;
        Duration = duration;
        StartTime = startTime;
        EndTime = endTime;
        Outcome = outcome;
        TestProjectDirectory = testProjectDirectory;
        TestOutputFile = testOutputFile;
        Codebase = codebase;
        StackTrace = stackTrace;
        ErrorMessage = errorMessage;
        StdOut = stdOut;

        var testNameParts = fullyQualifiedTestName.Split('.');

        if (testNameParts.Length > 1)
        {
            var testName = testNameParts[^1];
            var className = testNameParts[^2];
            var fullyQualifiedClassName = string.Join(".", testNameParts.Take(testNameParts.Length - 1));
            var @namespace = string.Join(".", testNameParts.Take(testNameParts.Length - 2));

            // only infer these if fullyQualifiedTestName is typical of .NET tests
            if (!fullyQualifiedClassName.Contains(" ") )
            {
                TestName = testName;
                ClassName = className;
                FullyQualifiedClassName = fullyQualifiedClassName;
                Namespace = @namespace;
            }
        }

        if (TestName is null)
        {
            TestName = FullyQualifiedTestName;
        }
    }

    public string Namespace { get; set; }
    public string TestName { get; set; }
    public string FullyQualifiedTestName { get; }
    public string FullyQualifiedClassName { get; set; }
    public string ComputerName { get; set; }
    public string ClassName { get; set; }

    public TestOutcome Outcome { get; }

    public DirectoryInfo TestProjectDirectory { get; }
    public FileInfo TestOutputFile { get; }
    public FileInfo Codebase { get; }

    public DateTimeOffset? StartTime { get; }
    public DateTimeOffset? EndTime { get; }
    public TimeSpan? Duration { get; }

    public string ErrorMessage { get; set; }
    public string StackTrace { get; }
    public string StdOut { get; set; }

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