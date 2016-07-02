using System;
using System.Collections.Generic;

namespace JungleBus.Aws.Sns
{
    /// <summary>
    /// Client for talking to SNS
    /// </summary>
    public interface ISnsClient : IDisposable
    {
        /// <summary>
        /// Publishes the serialized message
        /// </summary>
        /// <param name="message">Serialized Message</param>
        /// <param name="type">Payload type</param>
        void Publish(string message, Type type);

        /// <summary>
        /// Setups the bus for publishing the given message types
        /// </summary>
        /// <param name="messageTypes">Message types</param>
        void SetupMessagesForPublishing(IEnumerable<Type> messageTypes);
    }
}