using System;

namespace JungleBus.Messaging
{
    /// <summary>
    /// Result returns from the message processor when processing a message
    /// </summary>
    public class MessageProcessingResult
    {
        /// <summary>
        /// Gets or sets a flag indicating whether the message was
        /// successfully processed
        /// </summary>
        public bool WasSuccessful { get; set; }
        
        /// <summary>
        /// Gets or sets the exception, if any, from message processing
        /// </summary>
        public Exception Exception { get; set; }
    }
}
