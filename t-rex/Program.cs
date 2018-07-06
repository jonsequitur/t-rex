using System;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace TRex.CommandLine
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            await CommandLine.Parser.InvokeAsync(args);
        }
    }
}
