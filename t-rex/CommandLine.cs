using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TRexLib;

namespace TRex.CommandLine
{
    public class CommandLine
    {
        private readonly string[] args;

        public CommandLine(string args) :
            this(args.Split(
                     new[] { ' ' },
                     StringSplitOptions.RemoveEmptyEntries))
        {
        }

        public CommandLine(string[] args)
        {
            this.args = args;
        }

        public TestResultSet Invoke()
        {
            var files = new List<FileInfo>();
            var testResults = new List<TestResult>();

            if (args.Length == 0)
            {
                // search for files
                files.AddRange(SearchDirectory(Directory.GetCurrentDirectory()));
            }
            else if (args.Length == 1)
            {
                var path = args.Single();

                if (File.Exists(path))
                {
                    files.Add(new FileInfo(path));
                }

                if (Directory.Exists(path))
                {
                    files.AddRange(SearchDirectory(path));
                }
            }

            foreach (var file in files)
            {
                testResults.AddRange(file.Parse());
            }

            return new TestResultSet(testResults);
        }

        private IEnumerable<FileInfo> SearchDirectory(string path)
        {
            var allFiles = new DirectoryInfo(path)
                .GetFiles("*.trx", SearchOption.AllDirectories)
                .GroupBy(f => f.Directory.FullName);

            foreach (var folder in allFiles)
            {
                yield return folder.OrderBy(f => f.LastWriteTime).Last();
            }
        }
    }
}