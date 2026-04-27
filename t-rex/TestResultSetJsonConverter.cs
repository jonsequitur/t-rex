using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using TRexLib;

namespace TRex.CommandLine
{
    public class TestResultSetJsonConverter : JsonConverter<TestResultSet>
    {
        public override TestResultSet Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var items = JsonSerializer.Deserialize<List<TestResult>>(ref reader, options);
            return new TestResultSet(items);
        }

        public override void Write(Utf8JsonWriter writer, TestResultSet value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                JsonSerializer.Serialize(writer, item, options);
            }
            writer.WriteEndArray();
        }
    }
}
