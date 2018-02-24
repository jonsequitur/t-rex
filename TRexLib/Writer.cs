using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Recipes;

namespace TRexLib
{
    public static class Writer
    {
        public static void WriteResults(
            this TextWriter writer, 
            string label, 
            IEnumerable<TestResult> results)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
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

            writer.WriteLine(label);

            writer.WriteLine($"\tNAME\tRESULT\tSECONDS");

            foreach (var result in results)
            {
                writer.WriteLine($"\t{result.TestName}\t{result.Duration.IfNotNull().Then(d => d.TotalSeconds + "s").ElseDefault()}\t{result.TestProjectDirectory}");
            }
        }
    }
}
