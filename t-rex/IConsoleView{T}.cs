using System.CommandLine;
using System.Threading.Tasks;

namespace TRex.CommandLine
{
    public interface IConsoleView<in T>
    {
        Task WriteAsync(IConsole console, T testResults);
    }
}
