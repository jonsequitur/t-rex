using System;
using System.IO;
using Newtonsoft.Json;

namespace TRex.CommandLine
{
    public class DirectoryInfoJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type type) => type == typeof(DirectoryInfo);

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            if (reader.Value is string s)
            {
                return new DirectoryInfo(s);
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
            if (value is DirectoryInfo directory)
            {
                writer.WriteValue(directory.FullName);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }
}
