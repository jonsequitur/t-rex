using System;
using System.CommandLine;
using TRexLib;

namespace TRex.CommandLine
{
    internal static class ConsoleExtensions
    {
        public static ConsoleColor SetColorForOutcome(this IConsole console, TestOutcome outcome)
        {
            switch (outcome)
            {
                case TestOutcome.NotExecuted:
                case TestOutcome.Inconclusive:
                case TestOutcome.Pending:
                    return new ConsoleColor(console, System.ConsoleColor.Yellow);
                case TestOutcome.Failed:
                    return new ConsoleColor(console, System.ConsoleColor.Red);
                case TestOutcome.Passed:
                    return new ConsoleColor(console, System.ConsoleColor.Green);
                case TestOutcome.Timeout:
                    return new ConsoleColor(console, System.ConsoleColor.Magenta);

                default:
                    throw new NotSupportedException();
            }
        }
    }
}