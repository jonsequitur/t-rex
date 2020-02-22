using System;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace TRex.CommandLine
{
    public class Program
    {
        static async Task<int> Main(string[] args)
        {
           return await CommandLine.Parser.InvokeAsync(args);
        }
    }
}
