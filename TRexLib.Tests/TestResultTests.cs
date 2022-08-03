using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace TRexLib.Tests;

public class TestResultTests
{
    [Theory]
    [InlineData("namespace.class.test", "namespace")]
    [InlineData("deeper.namespace.class.test", "deeper.namespace")]
    [InlineData("still.deeper.namespace.class.test", "still.deeper.namespace")]
    public void Namespace_is_parsed_correctly(
        string fullyQualifiedTestName,
        string expectedNamespace)
    {
        var testResult = new TestResult(fullyQualifiedTestName, TestOutcome.NotExecuted);

        testResult.Namespace.Should().Be(expectedNamespace);
    }

    [Theory]
    [InlineData("namespace.class.test")]
    [InlineData("deeper.namespace.class.test")]
    [InlineData("still.deeper.namespace.class.test")]
    public void TestName_is_parsed_correctly(
        string fullyQualifiedTestName)
    {
        var testResult = new TestResult(fullyQualifiedTestName, TestOutcome.NotExecuted);

        testResult.TestName.Should().Be("test");
    }

    [Theory]
    [InlineData("namespace.class.test")]
    [InlineData("deeper.namespace.class.test")]
    [InlineData("still.deeper.namespace.class.test")]
    public void ClassName_is_parsed_correctly(
        string fullyQualifiedTestName)
    {
        var testResult = new TestResult(fullyQualifiedTestName, TestOutcome.NotExecuted);

        testResult.ClassName.Should().Be("class");
    }

    [Fact]
    public void Theory_test_is_parsed_correctly()
    {
        var testResult = new TestResult(
            "Microsoft.DotNet.Cli.MSBuild.IntegrationTests.GivenDotnetInvokesMSBuild.When_dotnet_command_invokes_msbuild_Then_env_vars_and_m_are_passed(command: \"build\")",
            outcome: TestOutcome.Passed);

        using var _ = new AssertionScope();

        testResult.TestName.Should().Be("When_dotnet_command_invokes_msbuild_Then_env_vars_and_m_are_passed(command: \"build\")");
        testResult.Namespace.Should().Be("Microsoft.DotNet.Cli.MSBuild.IntegrationTests");
        testResult.ClassName.Should().Be("GivenDotnetInvokesMSBuild");
    }

    [Theory]
    [InlineData("Cell 1: #r \"nuget:TRexLib\"")]
    [InlineData("Cell 1: Console.Write(\"Hello world.\";")]
    public void Inferred_properties_are_not_inferred_from_fully_qualified_test_name_if_they_do_not_match_dotnet_standards(
        string fullyQualifiedTestName)
    {
        var testResult = new TestResult(fullyQualifiedTestName, TestOutcome.NotExecuted);

        testResult.ClassName.Should().BeNull();
        testResult.Namespace.Should().BeNull();
        testResult.TestName.Should().Be(fullyQualifiedTestName);
    }
}