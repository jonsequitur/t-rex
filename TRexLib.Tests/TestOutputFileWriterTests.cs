using System.IO;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;
using Xunit.Abstractions;

namespace TRexLib.Tests;

public class TestOutputFileWriterTests
{
    private readonly ITestOutputHelper _output;

    public TestOutputFileWriterTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TrxWriter_writes_valid_TRX_file()
    {
        var original =
            new FileInfo(Path.Combine("TRXs", "2", "example2_Windows.trx"))
                .Parse();

        var roundTripped = WriteAndThenParse(original);

        using var _ = new AssertionScope();

        roundTripped.Count.Should().Be(original.Count);
        roundTripped.Passed.Count.Should().Be(original.Passed.Count);
        roundTripped.Passed.Count.Should().Be(original.Passed.Count);
        roundTripped.Failed.Count.Should().Be(original.Failed.Count);
    }
    
    private TestResultSet WriteAndThenParse(TestResultSet original)
    {
        using var writer = new StringWriter();

        new TestOutputFileWriter(writer).Write(original);

        _output.WriteLine(writer.ToString());

        var roundTripped = TestOutputFileParser.Parse(writer.ToString());
        return roundTripped;
    }
}