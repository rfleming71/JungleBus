using JungleBus.IoC;

namespace JungleBus.Configuration
{
    /// <summary>
    /// General bus configuration settings
    /// </summary>
    public interface IBusConfiguration
    {
        /// <summary>
        /// Gets or sets the service locator for message handlers 
        /// </summary>
        IObjectBuilder ObjectBuilder { get; set; }

        /// <summary>
        /// Gets or sets the inbound message settings
        /// </summary>
        ReceiveConfiguration Receive { get; set; }

        /// <summary>
        /// Gets or sets the outbound message settings
        /// </summary>
        SendConfiguration Send { get; set; }
    }
}
