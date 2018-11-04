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

        public ConsoleColor BackgroundColor { get; set; }

        public void SetOut(TextWriter writer)
        {
            Out = writer;
            IsOutputRedirected = true;
        }

        public Region GetRegion() => new Region(120, 80, 0, 0);

        public void SetCursorPosition(int left, int top)
        {
            CursorLeft = left;
            CursorTop = top;
        }


        public void TryEnableVirtualTerminal()
        {
            IsVirtualTerminal = true;
        }

        public bool IsVirtualTerminal { get; private set; }

        public TextWriter Out { get; private set; }

        public virtual ConsoleColor ForegroundColor { get; set; }

        public int CursorLeft { get; set; }

        public int CursorTop { get; set; }

        public bool IsOutputRedirected { get; private set; }

        public bool IsErrorRedirected { get; private set; }

        public bool IsInputRedirected { get; private set; }

        public virtual void ResetColor()
        {
        }

        public void Dispose()
        {
        }
    }
}
