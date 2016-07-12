using Common.Logging;

namespace JungleBus.Messaging
{
    /// <summary>
    /// Logs messages flowing in and out of the bus
    /// </summary>
    public class MessageLogger : IMessageLogger
    {
        /// <summary>
        /// Outbound logger instance
        /// </summary>
        private static ILog _outboundLogger = LogManager.GetLogger("JungleBus.MessageLogger.Recieve");

        /// <summary>
        /// Inbound logger instance
        /// </summary>
        private static ILog _inboundLogger = LogManager.GetLogger("JungleBus.MessageLogger.Publish");

        /// <summary>
        /// Logs messages received by the bus
        /// </summary>
        /// <param name="messageBody">Message body</param>
        /// <param name="messageType">Message type</param>
        public void InboundLogMessage(string messageBody, string messageType)
        {
            _outboundLogger.InfoFormat("Type: {0} Body: {1}", messageType, messageBody);
        }

        /// <summary>
        /// Logs messages being sent by the bus
        /// </summary>
        /// <param name="messageBody">Message body</param>
        /// <param name="messageType">Message type</param>
        public void OutboundLogMessage(string messageBody, string messageType)
        {
            _inboundLogger.InfoFormat("Type: {0} Body: {1}", messageType, messageBody);
        }
    }
}
