using System.CommandLine;
using System.CommandLine.IO;
using TRexLib;

namespace TRex.CommandLine
{
    internal static class View
    {
        internal static void WriteSummary(IConsole console, TestResultSet testResults)
        {
            console.Out.WriteLine("SUMMARY:");

            using (console.SetColorForOutcome(TestOutcome.Passed))
            {
                console.Out.Write($"Passed: {testResults.Passed.Count}, ");
            }

            using (console.SetColorForOutcome(TestOutcome.Failed))
            {
                console.Out.Write($"Failed: {testResults.Failed.Count}, ");
            }

            using (console.SetColorForOutcome(TestOutcome.NotExecuted))
            {
                console.Out.WriteLine($"Not run: {testResults.NotExecuted.Count}");
            }
        }
    }
}