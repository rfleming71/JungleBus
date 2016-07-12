namespace JungleBus.Messaging
{
    /// <summary>
    /// Message logger that does not actually log
    /// </summary>
    public class NoOpMessageLogger : IMessageLogger
    {
        /// <summary>
        /// Logs messages received by the bus
        /// </summary>
        /// <param name="messageBody">Message body</param>
        /// <param name="messageType">Message type</param>
        public void InboundLogMessage(string messageBody, string messageType)
        {
        }

        /// <summary>
        /// Logs messages being sent by the bus
        /// </summary>
        /// <param name="messageBody">Message body</param>
        /// <param name="messageType">Message type</param>
        public void OutboundLogMessage(string messageBody, string messageType)
        {
        }
    }
}
