using System.IO;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using Xunit.Abstractions;

namespace TRexLib.Tests;

public class TestOutputDocumentWriterTests
{
    private readonly ITestOutputHelper _output;

    public TestOutputDocumentWriterTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void It_writes_TRX_file_that_can_be_parsed_correctly_by_parser()
    {
        var original =
            new FileInfo(Path.Combine("TRXs", "2", "example2_Windows.trx"))
                .Parse();

        var roundTripped = WriteAndThenParse(original);

        using var _ = new AssertionScope();

        roundTripped.Count.Should().Be(original.Count);
        roundTripped.NotExecuted.Count.Should().Be(original.NotExecuted.Count);
        roundTripped.Passed.Count.Should().Be(original.Passed.Count);
        roundTripped.Failed.Count.Should().Be(original.Failed.Count);
    }

    [Fact]
    public void Test_run_name_is_written()
    {
        var original = new TestResultSet
        {
            TestRunName = "the test run"
        };

        var roundTripped = WriteAndThenParse(original);

        roundTripped.TestRunName.Should().Be("the test run");
    }

    [Fact]
    public void Stack_trace_is_written()
    {
        var original = new TestResultSet
        {
            new("the test name", TestOutcome.Failed, stackTrace: "the stack trace")
        };

        var roundTripped = WriteAndThenParse(original);

        roundTripped[0].StackTrace.Should().Be("the stack trace");
    }

    [Fact]
    public void Standard_out_is_written()
    {
        var original = new TestResultSet
        {
            new("the test name", TestOutcome.Failed, stdOut: "the stdout")
        };

        var roundTripped = WriteAndThenParse(original);

        roundTripped[0].StdOut.Should().Be("the stdout");
    }

    [Fact]
    public void Error_message_is_written()
    {
        var original = new TestResultSet
        {
            new("the test name", TestOutcome.Failed, errorMessage: "the stdout")
        };

        var roundTripped = WriteAndThenParse(original);

        roundTripped[0].ErrorMessage.Should().Be("the stdout");
    }

    private TestResultSet WriteAndThenParse(TestResultSet original)
    {
        using var writer = new StringWriter();

        new TestOutputDocumentWriter(writer).Write(original);

        _output.WriteLine(writer.ToString());

        var roundTripped = TestOutputDocumentParser.Parse(writer.ToString());

        return roundTripped;
    }
}