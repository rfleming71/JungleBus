using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JungleBus.Messaging
{
    /// <summary>
    /// Client for talking to queues
    /// </summary>
    public interface IMessageQueue
    {
        /// <summary>
        /// Gets or sets the message parser for inbound messages
        /// </summary>
        IMessageParser MessageParser { get; set; }

        /// <summary>
        /// Retrieve messages from the underlying queue
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Messages or empty</returns>
        Task<IEnumerable<TransportMessage>> GetMessages(CancellationToken cancellationToken);

        /// <summary>
        /// Removes a message from the queue
        /// </summary>
        /// <param name="message">Message to remove</param>
        void RemoveMessage(TransportMessage message);

        /// <summary>
        /// Subscribe the queue to the given message types
        /// </summary>
        /// <param name="messageTypes">Message to subscribe to</param>
        void Subscribe(IEnumerable<Type> messageTypes);

        /// <summary>
        /// Adds the message to the queue
        /// </summary>
        /// <param name="message">Message to add to the queue</param>
        void AddMessage(string message);
    }
}
