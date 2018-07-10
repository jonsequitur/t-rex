using System;
using FluentAssertions;
using Xunit;

namespace TRexLib.Tests
{
    public class TestResultTests
    {
        [Theory]
        [InlineData("class.test", "")]
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
        [InlineData("class.test")]
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
        [InlineData("class.test")]
        [InlineData("namespace.class.test")]
        [InlineData("deeper.namespace.class.test")]
        [InlineData("still.deeper.namespace.class.test")]
        public void ClassName_is_parsed_correctly(
            string fullyQualifiedTestName)
        {
            var testResult = new TestResult(fullyQualifiedTestName, TestOutcome.NotExecuted);

            testResult.ClassName.Should().Be("class");
        }
    }
}
