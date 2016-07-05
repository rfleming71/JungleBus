using JungleBus.Interfaces;

namespace JungleBus.Messaging
{
    /// <summary>
    /// Processes inbound messages and call the event handlers
    /// </summary>
    public interface IMessageProcessor
    {
        /// <summary>
        /// Processes inbound message and call the event handlers
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="busInstance">Instance of the bus to pass to event handlers</param>
        /// <returns>True is all event handles succeeded</returns>
        bool ProcessMessage(TransportMessage message, IBus busInstance);
    }
}