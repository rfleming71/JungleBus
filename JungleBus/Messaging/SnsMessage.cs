using System.Collections.Generic;

namespace JungleBus.Messaging
{
    /// <summary>
    /// DTO for decoding SNS messages
    /// </summary>
    internal class SnsMessage
    {
        /// <summary>
        /// Gets or sets the message body
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the message attributes
        /// </summary>
        public Dictionary<string, MessageAttribute> MessageAttributes { get; set; }
    }
}
