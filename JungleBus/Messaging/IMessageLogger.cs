namespace JungleBus.Messaging
{
    public interface IMessageLogger
    {
        void InboundLogMessage(string messageBody, string messageType);
        void OutboundLogMessage(string messageBody, string messageType);
    }
}