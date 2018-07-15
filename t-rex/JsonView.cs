using System.CommandLine;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TRexLib;

namespace TRex.CommandLine
{
    public class JsonView : IConsoleView<TestResultSet>
    {
        public async Task WriteAsync(IConsole console, TestResultSet testResults)
        {
            var json = JsonConvert.SerializeObject(
                testResults,
                Formatting.Indented,
                new FileInfoJsonConverter(),
                new DirectoryInfoJsonConverter());

            await console.Out.WriteAsync(json);
        }
    }
}
