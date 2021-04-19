namespace NetUlid
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public sealed class UlidJsonConverter : JsonConverter<Ulid>
    {
        public override Ulid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var json = reader.GetString();

            if (json == null)
            {
                // This should already handled by framework.
                throw new ArgumentException($"The JSON value is null.", nameof(reader));
            }

            try
            {
                return Ulid.Parse(json);
            }
            catch (FormatException ex)
            {
                throw new JsonException($"'{json}' is not a valid ULID.", ex);
            }
        }

        public override void Write(Utf8JsonWriter writer, Ulid value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
