using System;

namespace TRex.CommandLine
{
    internal class OutputColor : IDisposable
    {
        private readonly ConsoleColor originalColor;

        public OutputColor(ConsoleColor foregroundColor)
        {
            originalColor = Console.ForegroundColor;
            Console.ForegroundColor = foregroundColor;
        }

        public void Dispose()
        {
            Console.ForegroundColor = originalColor;
        }
    }
}