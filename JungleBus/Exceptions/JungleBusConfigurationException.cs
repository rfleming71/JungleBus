namespace JungleBus.Exceptions
{
    /// <summary>
    /// An issue with the configuration as occurred
    /// </summary>
    public class JungleBusConfigurationException : JungleBusException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JungleBusConfigurationException" /> class.
        /// </summary>
        /// <param name="configurationSetting">Setting that caused the exception</param>
        /// <param name="message">Error message</param>
        public JungleBusConfigurationException(string configurationSetting, string message)
            : base(string.Format("Setting: {0} Message: {1}", configurationSetting, message))
        {
        }
    }
}
