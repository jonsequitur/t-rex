using System;
using TRexLib;

namespace TRex.CommandLine
{
    public class Program
    {
        static void Main(string[] args)
        {
            var testResults = new CommandLine(args).Invoke();

            new SummaryWriter(Console.Out).Write(testResults);
        }
    }
}
