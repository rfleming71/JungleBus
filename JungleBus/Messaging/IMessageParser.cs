using Amazon.SQS.Model;

namespace JungleBus.Messaging
{
    /// <summary>
    /// Parses out messages from the input queue
    /// </summary>
    public interface IMessageParser
    {
        /// <summary>
        /// Parse the Amazon SQS message
        /// </summary>
        /// <param name="message">Message to parse</param>
        /// <returns>Parsed message</returns>
        TransportMessage ParseMessage(Message message);
    }
}