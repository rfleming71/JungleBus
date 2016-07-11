using JungleBus.Interfaces.IoC;

namespace JungleBus.Configuration
{
    /// <summary>
    /// Interface for configuring the object builder
    /// </summary>
    public interface IConfigureMessageSerializer
    {
        /// <summary>
        /// Gets or sets the service locator for message handlers 
        /// </summary>
        IObjectBuilder ObjectBuilder { get; }
    }
}
