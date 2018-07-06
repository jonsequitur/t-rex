using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Recipes;
using TRexLib;

namespace TRex.CommandLine
{
    internal static class Writer
    {
        public static async Task WriteResults(
            this IConsole console,
            string label,
            IEnumerable<TestResult> results)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            if (label == null)
            {
                throw new ArgumentNullException(nameof(label));
            }

            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            if (!results.Any())
            {
                return;
            }

            await console.Out.WriteLineAsync(label);

            await console.Out.WriteLineAsync($"\tNAME\tRESULT\tSECONDS");

            foreach (var result in results)
            {
                await console.Out.WriteLineAsync(
                    $"\t{result.TestName}\t{result.Duration.IfNotNull().Then(d => d.TotalSeconds + "s").ElseDefault()}\t{result.TestProjectDirectory}");
            }
        }
    }
}
