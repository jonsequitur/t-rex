using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TRexLib;

[TypeFormatterSource(typeof(TypeFormatterSource))]
public class TestResultSet : IReadOnlyList<TestResult>
{
    private readonly List<TestResult> _all = new();
    private readonly List<TestResult> _passed = new();
    private readonly List<TestResult> _failed = new();
    private readonly List<TestResult> _notExecuted = new();

    public TestResultSet(IEnumerable<TestResult> testResults = null)
    {
        if (testResults is not null)
        {
            foreach (var testResult in testResults.OrderBy(r => r.FullyQualifiedTestName))
            {
                Add(testResult);
            }
        }
    }

    public IReadOnlyCollection<TestResult> Passed => _passed;

    public IReadOnlyCollection<TestResult> Failed => _failed;

    public IReadOnlyCollection<TestResult> NotExecuted => _notExecuted;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<TestResult> GetEnumerator() => _all.GetEnumerator();

    public static TestResultSet Create(IEnumerable<FileInfo> files)
    {
        var testResults = new List<TestResult>();

        foreach (var file in files)
        {
            testResults.AddRange(file.Parse());
        }

        return new TestResultSet(testResults);
    }

    public static TestResultSet Create(string path, bool latestOnly = true) => Create(FindTrxFiles(path, latestOnly));

    public static IEnumerable<FileInfo> FindTrxFiles(string path, bool latestOnly = true)
    {
        var allFiles = new DirectoryInfo(path)
                       .GetFiles("*.trx", SearchOption.AllDirectories)
                       .GroupBy(f => f.Directory?.FullName);

        foreach (var folder in allFiles)
        {
            if (latestOnly)
            {
                yield return folder.OrderBy(f => f.LastWriteTime).Last();
            }
            else
            {
                foreach (var fileInfo in folder)
                {
                    yield return fileInfo;
                }
            }
        }
    }

    public int Count => _all.Count;

    public TestResult this[int index] => _all[index];

    public void Add(TestResult testResult)
    {
        _all.Add(testResult);

        switch (testResult.Outcome)
        {
            case TestOutcome.Passed:
                _passed.Add(testResult);
                break;
            case TestOutcome.Failed:
                _failed.Add(testResult);
                break;
            case TestOutcome.NotExecuted:
                _notExecuted.Add(testResult);
                break;
        }
    }
}