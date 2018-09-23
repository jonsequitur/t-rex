using System;
using System.CommandLine;
using System.CommandLine.Rendering;
using System.IO;

namespace TRexLib.Tests
{
    public class TestConsole : IConsole
    {
        private bool _isVirtualTerminalMode;

        public TestConsole()
        {
            Error = new StringWriter();
            Out = new StringWriter();
        }

        public TextWriter Error { get; }

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

        public bool IsVirtualTerminal()
        {
            return _isVirtualTerminalMode;
        }

        public void TryEnableVirtualTerminal()
        {
            _isVirtualTerminalMode = true;
        }

        public TextWriter Out { get; private set; }

        public virtual ConsoleColor ForegroundColor { get; set; }

        public int CursorLeft { get; set; }

        public int CursorTop { get; set; }

        public bool IsOutputRedirected { get; private set; }

        public bool IsErrorRedirected { get; private set; }

        public bool IsInputRedirected { get; private set; }

        public int WindowWidth { get; set; } = 80;

        public virtual void ResetColor()
        {
        }

        public void Dispose()
        {
        }
    }
}
