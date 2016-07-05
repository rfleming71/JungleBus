using JungleBus.Interfaces.IoC;
using JungleBus.IoC;

namespace JungleBus.Configuration
{
    /// <summary>
    /// Bus configuration settings
    /// </summary>
    internal class BusConfiguration : IConfigureObjectBuilder, IConfigureEventPublishing, IConfigureEventReceiving
    {
        /// <summary>
        /// Gets or sets the service locator for message handlers 
        /// </summary>
        public IObjectBuilder ObjectBuilder { get; set; }

        /// <summary>
        /// Gets or sets the inbound message settings
        /// </summary>
        public ReceiveConfiguration Receive { get; set; }

        /// <summary>
        /// Gets or sets the outbound message settings
        /// </summary>
        public SendConfiguration Send { get; set; }
    }
}
