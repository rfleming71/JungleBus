using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Common.Logging;
using JungleBus.Interfaces;
using JungleBus.Interfaces.IoC;

namespace JungleBus.Messaging
{
    /// <summary>
    /// Responsible for handling transport messages and calling the appropriate message handlers
    /// </summary>
    internal class MessageProcessor : IMessageProcessor
    {
        /// <summary>
        /// Instance of the logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(MessageProcessor));

        /// <summary>
        /// Collection of known message handlers
        /// </summary>
        private readonly Dictionary<Type, HashSet<Type>> _handlerTypes;

        /// <summary>
        /// Configured object builder
        /// </summary>
        private readonly IObjectBuilder _objectBuilder;

        /// <summary>
        /// Collection of message fault handlers organized by message type
        /// </summary>
        private readonly Dictionary<Type, HashSet<Type>> _faultHandlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageProcessor" /> class.
        /// </summary>
        /// <param name="objectBuilder">Use to construct the message handlers</param>
        /// <param name="handlers">Collection of message handlers organized by message type</param>
        /// <param name="handlers">Collection of message fault handlers organized by message type</param>
        public MessageProcessor(IObjectBuilder objectBuilder, Dictionary<Type, HashSet<Type>> handlers, Dictionary<Type, HashSet<Type>> faultHandlers)
        {
            _objectBuilder = objectBuilder;
            _handlerTypes = handlers;
            _faultHandlers = faultHandlers;
        }

        /// <summary>
        /// Process the given message
        /// </summary>
        /// <param name="message">Transport message to be dispatched to the event handlers</param>
        /// <param name="busInstance">Instance of the send bus to inject into the event handlers</param>
        /// <returns>True is all event handlers processed successfully</returns>
        public bool ProcessMessage(TransportMessage message, IBus busInstance)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }
            
            if (_handlerTypes.ContainsKey(message.MessageType))
            {
                Log.TraceFormat("Processing {0} handlers for message type {1}", _handlerTypes[message.MessageType].Count, message.MessageTypeName);
                var handlerMethod = typeof(IHandleMessage<>).MakeGenericType(message.MessageType).GetMethod("Handle");
                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Required))
                {
                    var eventHandlerTasks = _handlerTypes[message.MessageType].Select(handlerType => Task.Run(() =>
                        {
                            using (IObjectBuilder childBuilder = _objectBuilder.GetNestedBuilder())
                            {
                                if (busInstance != null)
                                {
                                    childBuilder.RegisterInstance<IBus>(busInstance);
                                }

                                childBuilder.RegisterInstance(LogManager.GetLogger(handlerType));
                                Log.TraceFormat("Running handler {0}", handlerType.Name);
                                try
                                {
                                    var handler = childBuilder.GetValue(handlerType);
                                    handlerMethod.Invoke(handler, new object[] { message.Message });
                                }
                                catch (Exception ex)
                                {
                                    Log.ErrorFormat("Error in handling {0} with {1}", ex, message.MessageType.Name, handlerType.Name);
                                    return false;
                                }

                                return true;
                            }
                        })).ToArray();

                    var results = Task.WhenAll(eventHandlerTasks);
                    results.Wait();
                    if (results.Result.All(x => x))
                    {
                        transactionScope.Complete();
                        return true;
                    }
                }
            }
            else
            {
                Log.WarnFormat("Could not find a handler for {0}", message.MessageTypeName);
            }

            return false;
        }

        /// <summary>
        /// Processes inbound message that have faulted more than the retry limit
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="busInstance">Instance of the bus to pass to event handlers</param>
        public void ProcessFaultedMessage(TransportMessage message, IBus busInstance)
        {
            ProcessFaultedMessageHandlers(message, busInstance);
            if (message.MessageParsingSucceeded)
            {
                ProcessFaultedMessageHandlers(message.Message, busInstance);
            }
        }

        private void ProcessFaultedMessageHandlers(object message, IBus busInstance)
        {
            Type messageType = message.GetType();
            if (_faultHandlers.ContainsKey(messageType))
            {
                Log.TraceFormat("Processing {0} fault handlers for message type {1}", _faultHandlers[messageType].Count, messageType.FullName);
                var handlerMethod = typeof(IHandleMessageFaults<>).MakeGenericType(messageType).GetMethod("Handle");
                foreach (Type handlerType in _faultHandlers[messageType])
                {
                    using (IObjectBuilder childBuilder = _objectBuilder.GetNestedBuilder())
                    {
                        if (busInstance != null)
                        {
                            childBuilder.RegisterInstance<IBus>(busInstance);
                        }

                        childBuilder.RegisterInstance(LogManager.GetLogger(handlerType));
                        Log.TraceFormat("Running handler {0}", handlerType.Name);
                        try
                        {
                            var handler = childBuilder.GetValue(handlerType);
                            handlerMethod.Invoke(handler, new object[] { message });
                        }
                        catch (Exception ex)
                        {
                            Log.ErrorFormat("Error in handling message fault {0} with {1}", ex, messageType.Name, handlerType.Name);
                        }
                    }
                }
            }
        }
    }
}
