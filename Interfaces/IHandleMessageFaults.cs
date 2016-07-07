namespace JungleBus.Interfaces
{
    /// <summary>
    /// Interface to mark a class as an event fault handler for event type T
    /// </summary>
    /// <typeparam name="T">Event type to handle</typeparam>
    public interface IHandleMessageFaults<T>
    {
        /// <summary>
        /// Handle the message fault of type T
        /// </summary>
        /// <param name="message">Message to handle</param>
        void Handle(T message);
    }
}
