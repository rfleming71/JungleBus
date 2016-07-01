using JungleBus.Messaging;

namespace JungleBus.Configuration
{
    /// <summary>
    /// Configuration for the send bus
    /// </summary>
    public class SendConfiguration
    {
        /// <summary>
        /// Gets or sets the client to publish on
        /// </summary>
        public IMessagePublisher MessagePublisher { get; set; }
    }
}
