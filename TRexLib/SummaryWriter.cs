using System;
using System.IO;
using System.Linq;

namespace TRexLib
{
    public class SummaryWriter
    {
        private readonly TextWriter output;

        public SummaryWriter(TextWriter output)
        {
            if (output == null) 
            {
                throw new ArgumentNullException(nameof(output));
            }
            this.output = output;
        }

        public void Write(TestResultSet testResults)
        {
            using (ConsoleColor.Green())
            {
                output.WriteResults("PASSED", testResults.Passed);
            }

            using (ConsoleColor.Yellow())
            {
                output.WriteResults("NOT RUN", testResults.NotExecuted);
            }

            using (ConsoleColor.Red())
            {
                output.WriteResults("FAILED", testResults.Failed);
            }

            output.WriteLine($"\nSUMMARY:");

            using (ConsoleColor.Green())
            {
                output.Write($"Passed: {testResults.Passed.Count()}, ");
            }

            using (ConsoleColor.Red())
            {
                output.Write($"Failed: {testResults.Failed.Count()}, ");
            }

            using (ConsoleColor.Yellow())
            {
                output.WriteLine($"Not run: {testResults.NotExecuted.Count()}");
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
