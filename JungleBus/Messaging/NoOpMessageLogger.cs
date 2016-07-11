namespace JungleBus.Messaging
{
    public class NoOpMessageLogger : IMessageLogger
    {
        public void InboundLogMessage(string messageBody, string messageType)
        {
        }

        public void OutboundLogMessage(string messageBody, string messageType)
        {
        }
    }
}
