using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using TRexLib;

namespace TRex.CommandLine
{
    public class JsonView : IConsoleView<TestResultSet>
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            Converters =
            {
                new FileInfoJsonConverter(),
                new DirectoryInfoJsonConverter(),
                new TestResultSetJsonConverter()
            }
        };

        public Task WriteAsync(TextWriter console, TestResultSet testResults)
        {
            var json = JsonSerializer.Serialize(testResults, _options);

            console.Write(json);

            return Task.CompletedTask;
        }
    }
}