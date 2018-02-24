using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using System.Linq;
using TRex.CommandLine;
using Xunit;
using Xunit.Abstractions;

namespace TRexLib.Tests
{
    public class CommandLineTests
    {
        private readonly ITestOutputHelper output;

        public CommandLineTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void When_no_arguments_are_given_then_files_are_discovered_recursively()
        {
            var results = new CommandLine(@"").Invoke();

            output.WriteLine(Directory.GetCurrentDirectory());
            output.WriteLine(string.Join("\n", results.Select(r => r.TestOutputFile.FullName).Distinct()));

            var directories = results.Select(e => e.TestOutputFile).Select(f => f.Directory).ToArray();

            directories.Should().Contain(d => d.Name == "TRXs");
            directories.Should().Contain(d => d.Name == "1" && d.Parent.Name == "TRXs");
        }

        [Fact]
        public void When_one_argument_is_given_and_it_is_a_file_path_then_it_is_interpreted_as_a_file_path()
        {
            var filePath = new FileInfo(Path.Combine("TRXs", "example1_Windows.trx"))
                .FullName;

            var results = new CommandLine(filePath).Invoke();

            results.Should().HaveCount(2);
        }

        [Fact]
        public void When_multiple_TRX_files_exist_in_the_directory_only_the_latest_is_read()
        {
            var filePath = new DirectoryInfo(Path.Combine("TRXs", "2")).FullName;

            var results = new CommandLine(filePath).Invoke();

            results.Should().HaveCount(18);
        }
    }
}