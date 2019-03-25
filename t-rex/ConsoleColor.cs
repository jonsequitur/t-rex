using System;
using System.CommandLine.Rendering;

namespace TRex.CommandLine
{
    internal class TerminalColor : IDisposable
    {
        private readonly ITerminal terminal;
        private readonly ConsoleColor originalColor;

        public TerminalColor(
            ITerminal terminal,
            ConsoleColor foregroundColor)
        {
            this.terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
            originalColor = terminal.ForegroundColor;
            terminal.ForegroundColor = foregroundColor;
        }

        public void Dispose()
        {
            terminal.ForegroundColor = originalColor;
        }
    }
}
