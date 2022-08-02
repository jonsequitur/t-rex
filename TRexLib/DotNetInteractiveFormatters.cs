using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Net.WebUtility;

namespace TRexLib;

[AttributeUsage(AttributeTargets.Class)]
internal class TypeFormatterSourceAttribute : Attribute
{
    public TypeFormatterSourceAttribute(Type formatterSourceType)
    {
        FormatterSourceType = formatterSourceType;
    }

    public Type FormatterSourceType { get; }
}

internal class TypeFormatterSource
{
    public IEnumerable<object> CreateTypeFormatters()
    {
        yield return new TestResultPlainTextFormatter();
        yield return new TestResultSetPlainTextFormatter();
        yield return new TestResultSetHtmlFormatter();
    }
}

internal class TestResultPlainTextFormatter
{
    public string MimeType => "text/plain";

    public bool Format(object instance, TextWriter writer)
    {
        if (instance is not TestResult result)
        {
            return false;
        }

        writer.Write(result.ToString());

        return true;
    }
}

internal class TestResultSetPlainTextFormatter
{
    public string MimeType => "text/plain";

    public bool Format(object instance, TextWriter writer)
    {
        if (instance is not TestResultSet resultSet)
        {
            return false;
        }

        Write(resultSet.Passed, $"✅ Passed ({resultSet.Passed.Count})");

        Write(resultSet.Failed, $"❌ Failed ({resultSet.Failed.Count})");

        if (resultSet.NotExecuted.Any())
        {
            Write(resultSet.NotExecuted, $"⚠️ Not executed {resultSet.NotExecuted.Count}");
        }

        return true;

        void Write(IEnumerable<TestResult> testResultSet, string summary)
        {
            foreach (var result in testResultSet)
            {
                writer.WriteLine(result);
            }
        }
    }
}

internal class TestResultSetHtmlFormatter
{
    public string MimeType => "text/html";

    public bool Format(object instance, TextWriter writer)
    {
        if (instance is not TestResultSet resultSet)
        {
            return false;
        }

        Write(resultSet.Passed, $"✅ Passed ({resultSet.Passed.Count})");

        Write(resultSet.Failed, $"❌ Failed ({resultSet.Failed.Count})");

        if (resultSet.NotExecuted.Any())
        {
            Write(resultSet.NotExecuted, $"⚠️ Not executed {resultSet.NotExecuted.Count}");
        }

        return true;

        void Write(IEnumerable<TestResult> testResultSet, string summary)
        {
            writer.Write("<details>");
            writer.Write($"<summary>{HtmlEncode(summary)}</summary>");

            foreach (var result in testResultSet)
            {
                writer.Write("<div>");

                if (!string.IsNullOrEmpty(result.Output))
                {
                    var output = result.Output + "\n\n" + result.Stacktrace;
                    writer.Write("<details>");
                    writer.Write("<summary>");
                    WriteTestName(result);
                    writer.Write("</summary>");
                    writer.Write($"<pre>{HtmlEncode(output)}</pre>");
                    writer.Write("</details>");
                }
                else
                {
                    WriteTestName(result);
                }

                writer.Write("</div>");
            }

            writer.Write("</details>");
        }

        void WriteTestName(TestResult result)
        {
            writer.Write($"{HtmlEncode(result.ClassName)}.<b>{HtmlEncode(result.TestName)}</b>");
        }
    }
}
