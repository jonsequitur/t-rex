using System.IO;
using TRexLib;

namespace TRex.CommandLine
{
    internal static class View
    {
        internal static void WriteSummary(TextWriter console, TestResultSet testResults)
        {
            console.WriteLine("SUMMARY:");

            using (console.SetColorForOutcome(TestOutcome.Passed))
            {
                console.Write($"Passed: {testResults.Passed.Count}, ");
            }

            using (console.SetColorForOutcome(TestOutcome.Failed))
            {
                console.Write($"Failed: {testResults.Failed.Count}, ");
            }

            using (console.SetColorForOutcome(TestOutcome.NotExecuted))
            {
                console.WriteLine($"Not run: {testResults.NotExecuted.Count}");
            }
        }
    }
}