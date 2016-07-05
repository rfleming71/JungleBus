using System;

namespace JungleBus.Interfaces.Serialization
{
    /// <summary>
    /// Serializes and deserializes message for transport over the bus
    /// </summary>
    public interface IMessageSerializer
    {
        /// <summary>
        /// Serializes a message so if can be sent over the bus
        /// </summary>
        /// <param name="message">Message to serialize</param>
        /// <returns>Serialized object</returns>
        string Serialize(object message);

        /// <summary>
        /// Deserializes an object to type type
        /// </summary>
        /// <param name="message">Value to deserialize</param>
        /// <param name="type">Type to deserialize to</param>
        /// <returns>Deserialized object</returns>
        object Deserialize(string message, Type type);
    }
}
