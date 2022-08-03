using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace TRexLib.Tests;

public class TestOutputFileParserTests
{
    private readonly ITestOutputHelper output;

    public TestOutputFileParserTests(ITestOutputHelper output)
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

        results.Select(r => r.FullyQualifiedTestName)
               .Should()
               .BeEquivalentTo(new[]
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

        results.Single(r => r.FullyQualifiedTestName == "Microsoft.DotNet.Cli.Sln.Internal.Tests.GivenAnSlnFile.WhenGivenAValidPathItReadsAnSlnFile")
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

        results.Single(r => r.FullyQualifiedTestName == "Microsoft.DotNet.Cli.Sln.Internal.Tests.GivenAnSlnFile.WhenGivenAValidPathItReadsAnSlnFile")
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

        results.Single(r => r.FullyQualifiedTestName == "Microsoft.DotNet.Cli.Sln.Internal.Tests.GivenAnSlnFile.WhenGivenAValidPathItReadsAnSlnFile")
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

        results.Single(r => r.FullyQualifiedTestName == "Microsoft.DotNet.Cli.Sln.Internal.Tests.GivenAnSlnFile.WhenGivenAValidPathItReadsAnSlnFile")
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

        results.Single(r => r.FullyQualifiedTestName == "Microsoft.DotNet.Cli.Sln.Internal.Tests.GivenAnSlnFile.WhenGivenAValidPathItReadsModifiesThenWritesAnSln")
               .Outcome
               .Should()
               .Be(TestOutcome.Failed);
    }

    [Fact]
    public void The_path_to_a_Windows_test_project_is_parsed()
    {
        var results =
            new FileInfo(Path.Combine("TRXs", "example1_Windows.trx"))
                .Parse();

        results.Single(r => r.FullyQualifiedTestName == "Microsoft.DotNet.Cli.Sln.Internal.Tests.GivenAnSlnFile.WhenGivenAValidPathItReadsModifiesThenWritesAnSln")
               .TestProjectDirectory
               .FullName
               .Should()
               .Be(@"C:\dev\github\cli\test\Microsoft.DotNet.Cli.Sln.Internal.Tests\");
    }

    [Fact]
    public void The_path_to_a_Windows_test_dll_is_parsed()
    {
        var results =
            new FileInfo(Path.Combine("TRXs", "example1_Windows.trx"))
                .Parse();

        results.Single(r => r.FullyQualifiedTestName == "Microsoft.DotNet.Cli.Sln.Internal.Tests.GivenAnSlnFile.WhenGivenAValidPathItReadsModifiesThenWritesAnSln")
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

        results.Single(r => r.FullyQualifiedTestName == "Microsoft.DotNet.Cli.Utils.Tests.GivenARootedCommandResolver.It_escapes_CommandArguments_when_returning_a_CommandSpec")
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

        results.Single(r => r.FullyQualifiedTestName == "Microsoft.DotNet.Cli.Utils.Tests.GivenARootedCommandResolver.It_escapes_CommandArguments_when_returning_a_CommandSpec")
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

        var recombinedCount = results.Passed.Count + results.Failed.Count + results.NotExecuted.Count;

        output.WriteLine(new { recombinedCount }.ToString());

        results.Count.Should().Be(recombinedCount);
    }

    [Fact]
    public void Test_run_names_are_parsed_correctly()
    {
        var results =
            new FileInfo(Path.Combine("TRXs", "complex.trx"))
                .Parse();

        results.TestRunName.Should().Be("josequ@JOSEQU10 2022-06-13 12:30:01");
    }

    [Fact]
    public void Test_standard_out_is_read_correctly()
    {
        var results =
            new FileInfo(Path.Combine("TRXs", "complex.trx"))
                .Parse();

        results
            .Single(t => t.FullyQualifiedTestName == "System.CommandLine.Tests.ParserTests.Option_arguments_can_match_subcommands")
            .StdOut
            .Should()
            .Be("ParseResult: ![ testhost.net462.x86 [ -a <subcommand> ] ]");

        #region example

        /* 
             <UnitTestResult executionId="c53191b6-26be-47bb-8cab-7c26f01e06d2" testId="b8fd30f1-a08e-dee6-f707-844684686595" testName="" computerName="JOSEQU10" duration="00:00:00.0010000" startTime="2022-06-13T12:30:02.6126075-07:00" endTime="2022-06-13T12:30:02.6126075-07:00" testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" outcome="Passed" testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d" relativeResultsDirectory="c53191b6-26be-47bb-8cab-7c26f01e06d2">
      <Output>
        <StdOut>ParseResult: ![ testhost.net462.x86 [ -a &lt;subcommand&gt; ] ]</StdOut>
      </Output>
    </UnitTestResult>
         */

        #endregion
    }

    [Fact]
    public void Test_error_message_is_read_correctly()
    {
        var results =
            new FileInfo(Path.Combine("TRXs", "complex.trx"))
                .Parse();

        results
            .Single(t => t.FullyQualifiedTestName == "System.CommandLine.Tests.ParserTests+RootCommandAndArg0.When_parsing_a_string_array_input_then_a_full_path_to_an_executable_is_not_matched_by_the_root_command")
            .ErrorMessage
            .Should()
            .Be("System.FormatException : Index (zero based) must be greater than or equal to zero and less than the size of the argument list.");

        #region example

        /*
           <UnitTestResult executionId="9e8b3750-613a-4904-b2d4-b8a627c43137" testId="e5d247a1-4209-3c8f-ba15-876ef58051f0" testName="System.CommandLine.Tests.ParserTests+RootCommandAndArg0.When_parsing_a_string_array_input_then_a_full_path_to_an_executable_is_not_matched_by_the_root_command" computerName="JOSEQU10" duration="00:00:00.1070000" startTime="2022-06-13T12:30:02.5066063-07:00" endTime="2022-06-13T12:30:02.5066063-07:00" testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" outcome="Failed" testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d" relativeResultsDirectory="9e8b3750-613a-4904-b2d4-b8a627c43137">
      <Output>
        <ErrorInfo>
          <Message>System.FormatException : Index (zero based) must be greater than or equal to zero and less than the size of the argument list.</Message>
          <StackTrace>   at System.Text.StringBuilder.AppendFormatHelper(IFormatProvider provider, String format, ParamsArray args)&#xD;
   at System.String.FormatHelper(IFormatProvider provider, String format, ParamsArray args)&#xD;
   at System.String.Format(String format, Object[] args)&#xD;
   at FluentAssertions.Execution.MessageBuilder.FormatArgumentPlaceholders(String failureMessage, Object[] failureArgs)&#xD;
   at FluentAssertions.Execution.MessageBuilder.Build(String message, Object[] messageArgs, String reason, ContextDataItems contextData, String identifier, String fallbackIdentifier)&#xD;
   at FluentAssertions.Execution.AssertionScope.&lt;&gt;c__DisplayClass29_0.&lt;FailWith&gt;b__0()&#xD;
   at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)&#xD;
   at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)&#xD;
   at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)&#xD;
   at FluentAssertions.Collections.SelfReferencingCollectionAssertions`2.ContainSingle(Expression`1 predicate, String because, Object[] becauseArgs)&#xD;
   at System.CommandLine.Tests.ParserTests.RootCommandAndArg0.When_parsing_a_string_array_input_then_a_full_path_to_an_executable_is_not_matched_by_the_root_command() in C:\dev\command-line-api\src\System.CommandLine.Tests\ParserTests.RootCommandAndArg0.cs:line 45</StackTrace>
        </ErrorInfo>
      </Output>
    </UnitTestResult>
         */

        #endregion
    }

    [Fact]
    public void Test_stack_trace_is_read_correctly()
    {
        var results =
            new FileInfo(Path.Combine("TRXs", "complex.trx"))
                .Parse();

        var stacktrace = results
                         .Single(t => t.FullyQualifiedTestName == "System.CommandLine.Tests.ParserTests+RootCommandAndArg0.When_parsing_a_string_array_input_then_a_full_path_to_an_executable_is_not_matched_by_the_root_command")
                         .StackTrace;

        stacktrace
            .Should()
            .Be("   at System.Text.StringBuilder.AppendFormatHelper(IFormatProvider provider, String format, ParamsArray args)\r\n   at System.String.FormatHelper(IFormatProvider provider, String format, ParamsArray args)\r\n   at System.String.Format(String format, Object[] args)\r\n   at FluentAssertions.Execution.MessageBuilder.FormatArgumentPlaceholders(String failureMessage, Object[] failureArgs)\r\n   at FluentAssertions.Execution.MessageBuilder.Build(String message, Object[] messageArgs, String reason, ContextDataItems contextData, String identifier, String fallbackIdentifier)\r\n   at FluentAssertions.Execution.AssertionScope.<>c__DisplayClass29_0.<FailWith>b__0()\r\n   at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)\r\n   at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)\r\n   at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)\r\n   at FluentAssertions.Collections.SelfReferencingCollectionAssertions`2.ContainSingle(Expression`1 predicate, String because, Object[] becauseArgs)\r\n   at System.CommandLine.Tests.ParserTests.RootCommandAndArg0.When_parsing_a_string_array_input_then_a_full_path_to_an_executable_is_not_matched_by_the_root_command() in C:\\dev\\command-line-api\\src\\System.CommandLine.Tests\\ParserTests.RootCommandAndArg0.cs:line 45");



        #region example

        /*
   <UnitTestResult executionId="9e8b3750-613a-4904-b2d4-b8a627c43137" testId="e5d247a1-4209-3c8f-ba15-876ef58051f0" testName="System.CommandLine.Tests.ParserTests+RootCommandAndArg0.When_parsing_a_string_array_input_then_a_full_path_to_an_executable_is_not_matched_by_the_root_command" computerName="JOSEQU10" duration="00:00:00.1070000" startTime="2022-06-13T12:30:02.5066063-07:00" endTime="2022-06-13T12:30:02.5066063-07:00" testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" outcome="Failed" testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d" relativeResultsDirectory="9e8b3750-613a-4904-b2d4-b8a627c43137">
<Output>
<ErrorInfo>
  <Message>System.FormatException : Index (zero based) must be greater than or equal to zero and less than the size of the argument list.</Message>
  <StackTrace>   at System.Text.StringBuilder.AppendFormatHelper(IFormatProvider provider, String format, ParamsArray args)&#xD;
at System.String.FormatHelper(IFormatProvider provider, String format, ParamsArray args)&#xD;
at System.String.Format(String format, Object[] args)&#xD;
at FluentAssertions.Execution.MessageBuilder.FormatArgumentPlaceholders(String failureMessage, Object[] failureArgs)&#xD;
at FluentAssertions.Execution.MessageBuilder.Build(String message, Object[] messageArgs, String reason, ContextDataItems contextData, String identifier, String fallbackIdentifier)&#xD;
at FluentAssertions.Execution.AssertionScope.&lt;&gt;c__DisplayClass29_0.&lt;FailWith&gt;b__0()&#xD;
at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)&#xD;
at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)&#xD;
at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)&#xD;
at FluentAssertions.Collections.SelfReferencingCollectionAssertions`2.ContainSingle(Expression`1 predicate, String because, Object[] becauseArgs)&#xD;
at System.CommandLine.Tests.ParserTests.RootCommandAndArg0.When_parsing_a_string_array_input_then_a_full_path_to_an_executable_is_not_matched_by_the_root_command() in C:\dev\command-line-api\src\System.CommandLine.Tests\ParserTests.RootCommandAndArg0.cs:line 45</StackTrace>
</ErrorInfo>
</Output>
</UnitTestResult>
 */

        #endregion
    }
}