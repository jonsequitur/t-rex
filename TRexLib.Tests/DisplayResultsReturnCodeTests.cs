using System;
using System.CommandLine;
using System.CommandLine.Invocation;
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
        private readonly IConsole console = new TestConsole();
        private readonly ITestOutputHelper output;

        public DisplayResultsReturnCodeTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task When_all_tests_pass_then_the_result_code_is_0()
        {
            var directoryPath = new DirectoryInfo(Path.Combine("TRXs")).FullName;

            var result = await CommandLine.Parser.InvokeAsync($"--path \"{directoryPath}\" --filter *BlockingMemoryStreamTests*", console);

            result.Should().Be(0);
        }

        [Fact]
        public async Task When_any_tests_fail_then_the_result_code_is_1()
        {
            var directoryPath = new DirectoryInfo(Path.Combine("TRXs")).FullName;

            var result = await CommandLine.Parser.InvokeAsync($"--path \"{directoryPath}\"", console);

            result.Should().Be(1);
        }

        [Fact]
        public async Task When_no_tests_are_found_then_the_result_code_is_minus_1()
        {
            var directoryPath = new DirectoryInfo(Path.Combine("TRXs")).FullName;

            output.WriteLine($"directoryPath: {directoryPath}");

            var parser = CommandLine.Parser;

            var result = parser.Parse($"--path \"{directoryPath}\" --filter that-matches-nothing");

            output.WriteLine($"result: {result}");

            var exitCode = await parser.InvokeAsync(result, console);

            output.WriteLine("Out:");
            output.WriteLine(console.Out.ToString());

            output.WriteLine("Error:");
            output.WriteLine(console.Error.ToString());

            exitCode.Should().Be(-1);
        }
    }
}
