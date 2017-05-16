// <copyright file="MessageProcessor.cs">
//     The MIT License (MIT)
//
// Copyright(c) 2016 Ryan Fleming
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// </copyright>
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Transactions;
using Common.Logging;
using JungleBus.Interfaces;
using JungleBus.Interfaces.IoC;
using JungleBus.Interfaces.Statistics;

namespace JungleBus.Queue.Messaging
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
        /// Collection of message fault handlers organized by message type
        /// </summary>
        private readonly Dictionary<Type, HashSet<Type>> _faultHandlers;

        /// <summary>
        /// Use to construct the message handlers
        /// </summary>
        private readonly IObjectBuilder _objectBuilder;

        /// <summary>
        /// Function called before the message handler is invoked
        /// </summary>
        private readonly Action<IObjectBuilder> _preHandler = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageProcessor" /> class.
        /// </summary>
        /// <param name="handlers">Collection of message handlers organized by message type</param>
        /// <param name="faultHandlers">Collection of message fault handlers organized by message type</param>
        /// <param name="objectBuilder">Use to construct the message handlers</param>
        /// <param name="preHandler">Function called before the handler is invoked</param>
        public MessageProcessor(Dictionary<Type, HashSet<Type>> handlers, Dictionary<Type, HashSet<Type>> faultHandlers, IObjectBuilder objectBuilder, Action<IObjectBuilder> preHandler = null)
        {
            _handlerTypes = handlers;
            _faultHandlers = faultHandlers;
            _objectBuilder = objectBuilder;
            _preHandler = preHandler;
        }

        /// <summary>
        /// Process the given message
        /// </summary>
        /// <param name="message">Transport message to be dispatched to the event handlers</param>
        /// <returns>True is all event handlers processed successfully</returns>
        public MessageProcessingResult ProcessMessage(TransportMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            MessageProcessingResult result = new MessageProcessingResult();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            if (_handlerTypes.ContainsKey(message.MessageType))
            {
                Log.TraceFormat("Processing {0} handlers for message type {1}", _handlerTypes[message.MessageType].Count, message.MessageTypeName);
                var handlerMethod = typeof(IHandleMessage<>).MakeGenericType(message.MessageType).GetMethod("Handle");
                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Required))
                {
                    foreach (Type handlerType in _handlerTypes[message.MessageType])
                    {
                        using (IObjectBuilder childBuilder = _objectBuilder.GetNestedBuilder())
                        {
                            childBuilder.RegisterInstance(LogManager.GetLogger(handlerType));
                            if (_preHandler != null)
                            {
                                Log.TraceFormat("Running preHandler");
                                _preHandler(childBuilder);
                            }

                            Log.TraceFormat("Running handler {0}", handlerType.Name);
                            try
                            {
                                var handler = childBuilder.GetValue(handlerType);
                                handlerMethod.Invoke(handler, new object[] { message.Message });
                            }
                            catch (TargetInvocationException ex)
                            {
                                Log.ErrorFormat("Error in handling {0} with {1}", ex, message.MessageType.Name, handlerType.Name);
                                result.Exception = ex.InnerException ?? ex;
                            }
                            catch (Exception ex)
                            {
                                Log.ErrorFormat("Error in handling {0} with {1}", ex, message.MessageType.Name, handlerType.Name);
                                result.Exception = ex;
                            }
                        }
                    }

                    result.WasSuccessful = result.Exception == null;
                    if (result.WasSuccessful)
                    {
                        transactionScope.Complete();
                    }
                }
            }
            else
            {
                Log.WarnFormat("Could not find a handler for {0}", message.MessageTypeName);
                result.Exception = new Exception(string.Format("Could not find a handler for {0}", message.MessageTypeName));
            }

            watch.Stop();
            result.Runtime = watch.Elapsed;

            return result;
        }

        /// <summary>
        /// Processes inbound message that have faulted more than the retry limit
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="ex">Exception caused by the message</param>
        public void ProcessFaultedMessage(TransportMessage message, Exception ex)
        {
            ProcessFaultedMessageHandlers(message, ex);
            if (message.MessageParsingSucceeded)
            {
                ProcessFaultedMessageHandlers(message.Message, ex);
            }
        }

        /// <summary>
        /// Processes inbound message statistics
        /// </summary>
        /// <param name="statistics">Message statistics</param>
        public void ProcessMessageStatistics(IMessageStatistics statistics)
        {
            var recievers = _objectBuilder.GetValues<IWantMessageStatistics>();
            if (recievers != null)
            {
                foreach (IWantMessageStatistics reciever in recievers)
                {
                    reciever.RecieveStatisitics(statistics);
                }
            }
        }

        /// <summary>
        /// Calls the fault handler on the message object
        /// </summary>
        /// <param name="message">Message to process</param>
        /// <param name="messageException">Exception caused by the message</param>
        private void ProcessFaultedMessageHandlers(object message, Exception messageException)
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
                        childBuilder.RegisterInstance(LogManager.GetLogger(handlerType));
                        if (_preHandler != null)
                        {
                            Log.TraceFormat("Running preHandler");
                            _preHandler(childBuilder);
                        }

                        Log.TraceFormat("Running handler {0}", handlerType.Name);
                        try
                        {
                            var handler = childBuilder.GetValue(handlerType);
                            handlerMethod.Invoke(handler, new object[] { message, messageException });
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
