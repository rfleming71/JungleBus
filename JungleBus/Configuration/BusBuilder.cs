namespace JungleBus.Configuration
{
    /// <summary>
    /// Starting class for configuring the AWS Bus
    /// </summary>
    public static class BusBuilder
    {
        /// <summary>
        /// Create the default bus configuration
        /// </summary>
        /// <returns>Default bus configuration</returns>
        public static IConfigureObjectBuilder Create()
        {
            return new BusConfiguration() as IConfigureObjectBuilder;
        }
    }
}
