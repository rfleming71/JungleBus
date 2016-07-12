namespace JungleBus.Messaging
{
    /// <summary>
    /// Logs messages flowing in and out of the bus
    /// </summary>
    public interface IMessageLogger
    {
        /// <summary>
        /// Logs messages received by the bus
        /// </summary>
        /// <param name="messageBody">Message body</param>
        /// <param name="messageType">Message type</param>
        void InboundLogMessage(string messageBody, string messageType);

        /// <summary>
        /// Logs messages being sent by the bus
        /// </summary>
        /// <param name="messageBody">Message body</param>
        /// <param name="messageType">Message type</param>
        void OutboundLogMessage(string messageBody, string messageType);
    }
}