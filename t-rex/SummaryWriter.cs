using System;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;
using TRexLib;

namespace TRex.CommandLine
{
    public class SummaryWriter : IConsoleWriter
    {
        public async Task WriteAsync(IConsole console, TestResultSet testResults)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            if (testResults == null)
            {
                throw new ArgumentNullException(nameof(testResults));
            }

            using (ConsoleColor.Green())
            {
                await console.WriteResults("PASSED", testResults.Passed);
            }

            using (ConsoleColor.Yellow())
            {
                await console.WriteResults("NOT RUN", testResults.NotExecuted);
            }

            using (ConsoleColor.Red())
            {
                await console.WriteResults("FAILED", testResults.Failed);
            }

            console.Out.WriteLine($"\nSUMMARY:");

            using (ConsoleColor.Green())
            {
                await console.Out.WriteAsync($"Passed: {testResults.Passed.Count()}, ");
            }

            using (ConsoleColor.Red())
            {
                await console.Out.WriteAsync($"Failed: {testResults.Failed.Count()}, ");
            }

            using (ConsoleColor.Yellow())
            {
                await console.Out.WriteLineAsync($"Not run: {testResults.NotExecuted.Count()}");
            }
        }

        public class ConsoleColor : IDisposable
        {
            private readonly System.ConsoleColor originalColor;

            public ConsoleColor(System.ConsoleColor foregroundColor)
            {
                originalColor = Console.ForegroundColor;
                Console.ForegroundColor = foregroundColor;
            }

            public void Dispose()
            {
                Console.ForegroundColor = originalColor;
            }

            public static ConsoleColor Red() => new ConsoleColor(System.ConsoleColor.Red);

            public static ConsoleColor Green() => new ConsoleColor(System.ConsoleColor.Green);

            public static ConsoleColor Yellow() => new ConsoleColor(System.ConsoleColor.Yellow);
        }
    }
}
