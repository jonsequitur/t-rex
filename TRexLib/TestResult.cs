using System;
using System.IO;
using System.Linq;

namespace TRexLib
{
    public class TestResult
    {
        public string TestName { get; set; }
        public TimeSpan? Duration { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public TestOutcome Outcome { get; set; }
        public DirectoryInfo TestProjectDirectory { get; set; }
        public FileInfo TestOutputFile { get; set; }
        public FileInfo Codebase { get; set; }
    }
}
