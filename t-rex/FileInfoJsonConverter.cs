using System;
using System.IO;
using Newtonsoft.Json;

namespace TRex.CommandLine
{
    public class FileInfoJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type type) => type == typeof(FileInfo);

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            if (reader.Value is string s)
            {
                return new FileInfo(s);
            }

            if (reader.Value is null)
            {
                return null;
            }

            throw new NotSupportedException($"Cannot read value {reader.Value}");
        }

        public override void WriteJson(
            Newtonsoft.Json.JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            if (value is FileInfo FileInfo)
            {
                writer.WriteValue(FileInfo.FullName);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }
}
