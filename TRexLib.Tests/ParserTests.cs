using System;
using System.IO;
using FluentAssertions;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using System.Runtime.InteropServices;

namespace TRexLib.Tests
{
    public class ParserTests
    {
        private readonly ITestOutputHelper output;

        public ParserTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test_names_are_read_correctly()
        {
            var results =
                new FileInfo(Path.Combine("TRXs", "example1_Windows.trx"))
                    .Parse();

            results.Should().HaveCount(2);

            results.Select(r => r.TestName)
                   .ShouldBeEquivalentTo(new[]
                   {
                       "Microsoft.DotNet.Cli.Sln.Internal.Tests.GivenAnSlnFile.WhenGivenAValidPathItReadsAnSlnFile",
                       "Microsoft.DotNet.Cli.Sln.Internal.Tests.GivenAnSlnFile.WhenGivenAValidPathItReadsModifiesThenWritesAnSln"
                   });
        }

        [Fact]
        public void Test_durations_are_read_correctly()
        {
            var results =
                new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "TRXs", "example1_Windows.trx"))
                    .Parse();

            results.Single(r => r.TestName == "Microsoft.DotNet.Cli.Sln.Internal.Tests.GivenAnSlnFile.WhenGivenAValidPathItReadsAnSlnFile")
                   .Duration
                   .Should()
                   .Be(138.Milliseconds());
        }

        [Fact]
        public void Test_start_times_are_read_correctly()
        {
            var results =
                new FileInfo(Path.Combine("TRXs", "example1_Windows.trx"))
                    .Parse();

            results.Single(r => r.TestName == "Microsoft.DotNet.Cli.Sln.Internal.Tests.GivenAnSlnFile.WhenGivenAValidPathItReadsAnSlnFile")
                   .StartTime
                   .Should()
                   .Be(DateTimeOffset.Parse("2016-12-21T11:15:51.8308573-08:00"));
        }

        [Fact]
        public void Test_end_times_are_read_correctly()
        {
            var results =
                new FileInfo(Path.Combine("TRXs", "example1_Windows.trx"))
                    .Parse();

            results.Single(r => r.TestName == "Microsoft.DotNet.Cli.Sln.Internal.Tests.GivenAnSlnFile.WhenGivenAValidPathItReadsAnSlnFile")
                   .EndTime
                   .Should()
                   .Be(DateTimeOffset.Parse("2016-12-21T11:15:51.8308573-08:00"));
        }

        [Fact]
        public void Test_pass_outcomes_are_read_correctly()
        {
            var results =
                new FileInfo(Path.Combine("TRXs", "example1_Windows.trx"))
                    .Parse();

            results.Single(r => r.TestName == "Microsoft.DotNet.Cli.Sln.Internal.Tests.GivenAnSlnFile.WhenGivenAValidPathItReadsAnSlnFile")
                   .Outcome
                   .Should()
                   .Be(TestOutcome.Passed);
        }

        [Fact]
        public void Test_fail_outcomes_are_read_correctly()
        {
            var results =
                new FileInfo(Path.Combine("TRXs", "example1_Windows.trx"))
                    .Parse();

            results.Single(r => r.TestName == "Microsoft.DotNet.Cli.Sln.Internal.Tests.GivenAnSlnFile.WhenGivenAValidPathItReadsModifiesThenWritesAnSln")
                   .Outcome
                   .Should()
                   .Be(TestOutcome.Failed);
        }

        [Fact]
        public void The_path_to_a_Windows_test_project_is_parsed()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            var results =
                new FileInfo(Path.Combine("TRXs", "example1_Windows.trx"))
                    .Parse();

            results.Single(r => r.TestName == "Microsoft.DotNet.Cli.Sln.Internal.Tests.GivenAnSlnFile.WhenGivenAValidPathItReadsModifiesThenWritesAnSln")
                   .TestProjectDirectory
                   .FullName
                   .Should()
                   .Be(@"C:\dev\github\cli\test\Microsoft.DotNet.Cli.Sln.Internal.Tests\");
        }

        [Fact]
        public void The_path_to_a_Windows_test_dll_is_parsed()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            var results =
                new FileInfo(Path.Combine("TRXs", "example1_Windows.trx"))
                    .Parse();

            results.Single(r => r.TestName == "Microsoft.DotNet.Cli.Sln.Internal.Tests.GivenAnSlnFile.WhenGivenAValidPathItReadsModifiesThenWritesAnSln")
                   .Codebase
                   .FullName
                   .Should()
                   .Be(@"C:\dev\github\cli\test\Microsoft.DotNet.Cli.Sln.Internal.Tests\bin\Debug\netcoreapp1.0\Microsoft.DotNet.Cli.Sln.Internal.Tests.dll");
        }


        [Fact(Skip = "Path is calculated incorrectly on Windows")]
        public void The_path_to_an_OSX_test_project_is_parsed()
        {
            var results =
                new FileInfo(Path.Combine("TRXs", "1", "example1_OSX.trx"))
                    .Parse();

            results.Single(r => r.TestName == "Microsoft.DotNet.Cli.Utils.Tests.GivenARootedCommandResolver.It_escapes_CommandArguments_when_returning_a_CommandSpec")
                   .TestProjectDirectory
                   .FullName
                   .Should()
                   .Be(@"/Users/josequ/dev/cli/test/Microsoft.DotNet.Cli.Utils.Tests/");
        }

        [Fact(Skip = "Path is calculated incorrectly on Windows")]
        public void The_path_to_an_OSX_test_dll_is_parsed()
        {
            var results =
                new FileInfo(Path.Combine("TRXs", "1", "example1_OSX.trx"))
                    .Parse();

            results.Single(r => r.TestName == "Microsoft.DotNet.Cli.Utils.Tests.GivenARootedCommandResolver.It_escapes_CommandArguments_when_returning_a_CommandSpec")
                   .Codebase
                   .FullName
                   .Should()
                   .Be(@"/Users/josequ/dev/cli/test/Microsoft.DotNet.Cli.Utils.Tests/bin/Debug/netcoreapp1.0/Microsoft.DotNet.Cli.Utils.Tests.dll");
        }

        [Fact]
        public void Tests_do_not_appear_with_more_than_one_outcome()
        {
            var results =
                new FileInfo(Path.Combine("TRXs", "2", "example2_Windows.trx"))
                    .Parse();

            var recombinedCount = results.Passed.Count() + results.Failed.Count() + results.NotExecuted.Count();

            output.WriteLine(new {recombinedCount}.ToString());

            results.Count().Should().Be(recombinedCount);
        }
    }
}