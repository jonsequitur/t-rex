using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TRex.CommandLine
{
    public class DirectoryInfoJsonConverter : JsonConverter<DirectoryInfo>
    {
        public override DirectoryInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            return s is not null ? new DirectoryInfo(s) : null;
        }

        public override void Write(Utf8JsonWriter writer, DirectoryInfo value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.FullName);
        }
    }
}
