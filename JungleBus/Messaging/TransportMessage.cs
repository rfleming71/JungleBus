using System;

namespace JungleBus.Messaging
{
    /// <summary>
    /// Object for holding parsed message information
    /// </summary>
    public class TransportMessage
    {
        /// <summary>
        /// Gets or sets the body value
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the deserialized message
        /// </summary>
        /// <remarks>Will be null if parsing failed</remarks>
        public object Message { get; set; }

        /// <summary>
        /// Gets or sets the type name passed via the message
        /// </summary>
        public string MessageTypeName { get; set; }

        /// <summary>
        /// Gets or sets the type name passed via the message
        /// </summary>
        public Type MessageType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the message parsing succeeded
        /// </summary>
        public bool MessageParsingSucceeded { get; set; }

        /// <summary>
        /// Gets or sets the exception that was raised during the message parsing
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets the message handle
        /// </summary>
        public string ReceiptHandle { get; set; }

        /// <summary>
        /// Gets or sets the number of times this message has already been processed
        /// </summary>
        public int RetryCount { get; set; }
    }
}
