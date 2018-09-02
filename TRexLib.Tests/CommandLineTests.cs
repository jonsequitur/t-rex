using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using TRex.CommandLine;
using Xunit;

namespace TRexLib.Tests
{
    public class CommandLineTests
    {
        private readonly IConsole console = new TestConsole();

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

            var result = await CommandLine.Parser.InvokeAsync($"--path \"{directoryPath}\" --filter that-matches-nothing", console);

            result.Should().Be(-1);
        }
    }
}
