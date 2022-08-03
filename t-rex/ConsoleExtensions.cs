using System;
using System.CommandLine;
using System.CommandLine.IO;
using TRexLib;

namespace TRex.CommandLine
{
    internal static class ConsoleExtensions
    {
        public static IDisposable SetColor(this IConsole console, ConsoleColor color)
        {
            return new OutputColor(color);
        }

        public static IDisposable SetColorForOutcome(this IConsole console, TestOutcome outcome)
        {
            switch (outcome)
            {
                case TestOutcome.NotExecuted:
                case TestOutcome.Inconclusive:
                case TestOutcome.Pending:
                    return new OutputColor(ConsoleColor.Yellow);
                case TestOutcome.Failed:
                    return new OutputColor(ConsoleColor.Red);
                case TestOutcome.Passed:
                    return new OutputColor(ConsoleColor.Green);
                case TestOutcome.Timeout:
                    return new OutputColor(ConsoleColor.Magenta);

                default:
                    throw new NotSupportedException();
            }
        }

        internal static void WriteDuration(this IConsole console, TimeSpan? duration)
        {
            using (console.SetColor(ConsoleColor.Gray))
            {
                console.Out.Write($"({(duration ?? TimeSpan.Zero).TotalSeconds}s)");
            }
        }
    }
}