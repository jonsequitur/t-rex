using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TRex.CommandLine
{
    public class FileInfoJsonConverter : JsonConverter<FileInfo>
    {
        public override FileInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            return s is not null ? new FileInfo(s) : null;
        }

        public override void Write(Utf8JsonWriter writer, FileInfo value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.FullName);
        }
    }
}
