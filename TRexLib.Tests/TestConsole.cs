using System;
using System.CommandLine;
using System.IO;

namespace TRexLib.Tests
{
    public class TestConsole : IConsole
    {
        public TestConsole()
        {
            Error = new StringWriter();
            Out = new StringWriter();
        }

        public TextWriter Error { get; }

        public TextWriter Out { get; }

        public virtual ConsoleColor ForegroundColor { get; set; }

        public int WindowWidth { get; set; } = 80;

        public virtual void ResetColor()
        {
        }
    }
}