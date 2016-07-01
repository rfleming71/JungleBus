namespace JungleBus
{
    /// <summary>
    /// Bus that can be started to receive messages from the input queue
    /// </summary>
    public interface IRunJungleBus
    {
        /// <summary>
        /// Starts the bus receiving messages from the queue
        /// </summary>
        void StartReceiving();

        /// <summary>
        /// Stops the bus
        /// </summary>
        void StopReceiving();

        /// <summary>
        /// Creates an instance of the bus that can publish events
        /// </summary>
        /// <returns>Bus instance</returns>
        IBus CreateSendBus();
    }
}
