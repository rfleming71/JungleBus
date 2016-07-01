using System;

namespace JungleBus
{
    /// <summary>
    /// A bus that can publish messages
    /// </summary>
    public interface IBus
    {
        /// <summary>
        /// Publishes a message to the bus
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="message">Message to publish</param>
        void Publish<T>(T message);

        /// <summary>
        /// Publishes a message to the bus
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="messageBuilder">Function to initialize the message</param>
        void Publish<T>(Action<T> messageBuilder) where T : new();

        /// <summary>
        /// Send a message to the bus's input queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        void PublishLocal<T>(T message);

        /// <summary>
        /// Send a message to the bus's input queue
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="messageBuilder">Function to initialize the message</param>
        void PublishLocal<T>(Action<T> messageBuilder) where T : new();
    }
}
