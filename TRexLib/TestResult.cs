using System;
using System.IO;
using System.Linq;

namespace TRexLib
{
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
            FileInfo codebase = null)
        {
            FullyQualifiedTestName = fullyQualifiedTestName;
            Duration = duration;
            StartTime = startTime;
            EndTime = endTime;
            Outcome = outcome;
            TestProjectDirectory = testProjectDirectory;
            TestOutputFile = testOutputFile;
            Codebase = codebase;

            var testNameParts = fullyQualifiedTestName.Split('.');
            TestName = testNameParts[testNameParts.Length - 1];
            if (testNameParts.Length > 1)
            {
                ClassName = testNameParts[testNameParts.Length - 2];
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

        public string Namespace { get; }
        public string TestName { get; }
        public string ClassName { get; }
    }
}
