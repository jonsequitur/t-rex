using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using TRex.CommandLine;
using Xunit;
using Xunit.Abstractions;

namespace TRexLib.Tests
{
    public class DisplayResultsReturnCodeTests
    {
        private readonly ITestOutputHelper output;
        private CommandLineConfiguration _commandLineConfig;

        public DisplayResultsReturnCodeTests(ITestOutputHelper output)
        {
            this.output = output;
            _commandLineConfig = CommandLine.CommandLineConfig;
            _commandLineConfig.Output = new StringWriter();
            _commandLineConfig.Error = new StringWriter();
        }

        [Fact]
        public async Task When_all_tests_pass_then_the_result_code_is_0()
        {
            var directoryPath = new DirectoryInfo(Path.Combine("TRXs")).FullName;

            var result = await CommandLine.CommandLineConfig.InvokeAsync($"--path \"{directoryPath}\" --filter *BlockingMemoryStreamTests*");

            result.Should().Be(0);
        }

        [Fact]
        public async Task When_any_tests_fail_then_the_result_code_is_1()
        {
            var directoryPath = new DirectoryInfo(Path.Combine("TRXs")).FullName;

            var result = await CommandLine.CommandLineConfig.InvokeAsync($"--path \"{directoryPath}\"");

            result.Should().Be(1);
        }

        [Fact]
        public async Task When_no_tests_are_found_then_the_result_code_is_minus_1()
        {
            var directoryPath = new DirectoryInfo(Path.Combine("TRXs")).FullName;

            output.WriteLine($"directoryPath: {directoryPath}");

            var result = _commandLineConfig.RootCommand.Parse($"--path \"{directoryPath}\" --filter that-matches-nothing");

            output.WriteLine($"result: {result}");

            var exitCode = await result.InvokeAsync();

            output.WriteLine("Out:");
            output.WriteLine(_commandLineConfig.Output.ToString());

            output.WriteLine("Error:");
            output.WriteLine(_commandLineConfig.Output.ToString());

            exitCode.Should().Be(-1);
        }
    }
}