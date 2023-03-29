using System.CommandLine;
using System.IO;
using System.Threading.Tasks;

namespace TRex.CommandLine
{
    public interface IConsoleView<in T>
    {
        Task WriteAsync(TextWriter console, T testResults);
    }
}
