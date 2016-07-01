using System;
using System.Collections.Generic;

namespace JungleBus.Messaging
{
    /// <summary>
    /// Controls the actual bus
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// Setups the bus for publishing the given message types
        /// </summary>
        /// <param name="messageTypes">Message types</param>
        void SetupMessagesForPublishing(IEnumerable<Type> messageTypes);

        /// <summary>
        /// Publishes the serialized message
        /// </summary>
        /// <param name="message">Serialized Message</param>
        /// <param name="type">Payload type</param>
        void Publish(string message, Type type);

        /// <summary>
        /// Sends a message to the given queue
        /// </summary>
        /// <param name="messageString">Message to publish</param>
        /// <param name="type">Type of message to send</param>
        /// <param name="localMessageQueue">Queue to send to</param>
        void Send(string messageString, Type type, IMessageQueue localMessageQueue);
    }
}
