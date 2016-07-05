namespace JungleBus.Interfaces
{
    /// <summary>
    /// Interface to mark a class as an event handler for event type T
    /// </summary>
    /// <typeparam name="T">Event type to handle</typeparam>
    public interface IHandleMessage<T>
    {
        /// <summary>
        /// Handle the message of type T
        /// </summary>
        /// <param name="message">Message to handle</param>
        void Handle(T message);
    }
}
