using System.CommandLine;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TRexLib;

namespace TRex.CommandLine
{
    public class JsonView : IConsoleView<TestResultSet>
    {
        public Task WriteAsync(IConsole console, TestResultSet testResults)
        {
            var json = JsonConvert.SerializeObject(
                testResults,
                Formatting.Indented,
                new FileInfoJsonConverter(),
                new DirectoryInfoJsonConverter());

            console.Out.Write(json);

            return Task.CompletedTask;
        }
    }
}