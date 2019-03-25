using System;
using System.CommandLine;
using System.CommandLine.Rendering;
using Pocket;
using TRexLib;

namespace TRex.CommandLine
{
    internal static class ConsoleExtensions
    {
        public static IDisposable SetColor(this IConsole console, ConsoleColor color)
        {
            if (!(console is ITerminal terminal))
            {
                return Disposable.Empty;
            }

            return new TerminalColor(terminal, color);
        }

        public static IDisposable SetColorForOutcome(this IConsole console, TestOutcome outcome)
        {
            if (!(console is ITerminal terminal))
            {
                return Disposable.Empty;
            }

            switch (outcome)
            {
                case TestOutcome.NotExecuted:
                case TestOutcome.Inconclusive:
                case TestOutcome.Pending:
                    return new TerminalColor(terminal, ConsoleColor.Yellow);
                case TestOutcome.Failed:
                    return new TerminalColor(terminal, ConsoleColor.Red);
                case TestOutcome.Passed:
                    return new TerminalColor(terminal, ConsoleColor.Green);
                case TestOutcome.Timeout:
                    return new TerminalColor(terminal, ConsoleColor.Magenta);

                default:
                    throw new NotSupportedException();
            }
        }
    }
}