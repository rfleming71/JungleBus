using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace JungleBus.Serialization
{
    /// <summary>
    /// Serializes and deserializes message for transport over the bus
    /// using JSON.Net
    /// </summary>
    internal class JsonNetSerializer : IMessageSerializer
    {
        /// <summary>
        /// Instance of the JSON serializer
        /// </summary>
        private readonly JsonSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonNetSerializer" /> class.
        /// </summary>
        public JsonNetSerializer()
        {
            _serializer = JsonSerializer.CreateDefault();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonNetSerializer" /> class.
        /// </summary>
        /// <param name="settings">Custom JSON settings</param>
        public JsonNetSerializer(JsonSerializerSettings settings)
        {
            _serializer = JsonSerializer.CreateDefault(settings);
        }

        /// <summary>
        /// Deserializes an object to type type
        /// </summary>
        /// <param name="message">Value to deserialize</param>
        /// <param name="type">Type to deserialize to</param>
        /// <returns>Deserialized object</returns>
        public object Deserialize(string message, Type type)
        {
            using (StringReader stringReader = new StringReader(message))
            {
                using (var reader = new JsonTextReader(stringReader))
                {
                    return _serializer.Deserialize(reader, type);
                }
            }
        }

        /// <summary>
        /// Serializes a message so if can be sent over the bus
        /// </summary>
        /// <param name="message">Message to serialize</param>
        /// <returns>Serialized object</returns>
        public string Serialize(object message)
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                using (var jsonWriter = new JsonTextWriter(sw))
                {
                    jsonWriter.Formatting = _serializer.Formatting;
                    _serializer.Serialize(jsonWriter, message, null);
                }

                return sw.ToString();
            }
        }
    }
}
