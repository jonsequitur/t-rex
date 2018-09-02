using System;
using System.CommandLine;
using System.CommandLine.Rendering;
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

        public Region GetRegion() => new Region(120, 80, 0, 0);

        public void SetCursorPosition(int left, int top)
        {
            CursorLeft = left;
            CursorTop = top;
        }

        public TextWriter Out { get; }

        public virtual ConsoleColor ForegroundColor { get; set; }

        public int CursorLeft { get; set; }

        public int CursorTop { get; set; }

        public bool IsOutputRedirected { get; set; }

        public bool IsErrorRedirected { get; set; }

        public bool IsInputRedirected { get; set; }

        public int WindowWidth { get; set; } = 80;

        public virtual void ResetColor()
        {
        }
    }
}
