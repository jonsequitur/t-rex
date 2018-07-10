using System;
using System.CommandLine;

namespace TRex.CommandLine
{
    internal class ConsoleColor : IDisposable
    {
        private readonly IConsole console;
        private readonly System.ConsoleColor originalColor;

        public ConsoleColor(
            IConsole console,
            System.ConsoleColor foregroundColor)
        {
            this.console = console;
            originalColor = console.ForegroundColor;
            console.ForegroundColor = foregroundColor;
        }

        public void Dispose()
        {
            console.ForegroundColor = originalColor;
        }
    }
}
