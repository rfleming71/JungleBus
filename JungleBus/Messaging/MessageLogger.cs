using Common.Logging;

namespace JungleBus.Messaging
{
    public class MessageLogger : IMessageLogger
    {
        private static ILog _outboundLogger = LogManager.GetLogger("JungleBus.MessageLogger.Recieve");
        private static ILog _inboundLogger = LogManager.GetLogger("JungleBus.MessageLogger.Publish");

        public void InboundLogMessage(string messageBody, string messageType)
        {
            _outboundLogger.InfoFormat("Type: {0} Body: {1}", messageType, messageBody);
        }

        public void OutboundLogMessage(string messageBody, string messageType)
        {
            _inboundLogger.InfoFormat("Type: {0} Body: {1}", messageType, messageBody);
        }
    }
}
