namespace JungleBus.Messaging
{
    /// <summary>
    /// DTO for decoding SNS message attributes
    /// </summary>
    internal class MessageAttribute
    {
        /// <summary>
        /// Gets or sets the value type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public string Value { get; set; }
    }
}
