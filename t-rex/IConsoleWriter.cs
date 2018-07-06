using System.CommandLine;
using System.Threading.Tasks;
using TRexLib;

namespace TRex.CommandLine
{
    public interface IConsoleWriter
    {
        Task WriteAsync(IConsole console, TestResultSet testResults);
    }
}
