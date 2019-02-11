using System;
using System.Diagnostics;
using aspnetapp.Sys;
using Newtonsoft.Json;

namespace aspnetapp.Controllers
{
    public class EmailConverter : JsonConverter<Email>
    {
        private static readonly object _latch = new object();

        public override void WriteJson(JsonWriter writer, Email value, JsonSerializer serializer) => throw new NotImplementedException();

        public override Email ReadJson(JsonReader reader, Type objectType, Email existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = (string)reader.Value;
            var tokens = value.Split('@');
            ushort domainIndex;
            lock (_latch)
            {
                domainIndex = StringIndexer.Domains.GetOrAdd(tokens[1]);
            }
            Utf8String email = UnsafeStringContainer.GetString(tokens[0], false);
            return new Email(email, domainIndex);
        }
    }
}