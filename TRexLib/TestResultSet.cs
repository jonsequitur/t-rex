using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TRexLib
{
    public class TestResultSet : IEnumerable<TestResult>
    {
        private readonly IOrderedEnumerable<TestResult> all;

        public TestResultSet(IEnumerable<TestResult> testResults)
        {
            all = testResults.OrderBy(r => r.TestName);

            Passed = all.Where(r => r.Outcome == TestOutcome.Passed);
            Failed = all.Where(r => r.Outcome == TestOutcome.Failed);
            NotExecuted = all.Where(r => r.Outcome == TestOutcome.NotExecuted);
        }

        public IEnumerable<TestResult> Passed { get; }

        public IEnumerable<TestResult> Failed { get; }

        public IEnumerable<TestResult> NotExecuted { get; set; }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<TestResult> GetEnumerator() => all.GetEnumerator();
    }
}
