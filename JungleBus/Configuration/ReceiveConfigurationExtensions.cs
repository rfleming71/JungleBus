using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amazon;
using JungleBus.Aws.Sqs;
using JungleBus.Exceptions;
using JungleBus.Messaging;

namespace JungleBus.Configuration
{
    /// <summary>
    /// Extension Methods for configuration the bus
    /// </summary>
    public static class ReceiveConfigurationExtensions
    {
        /// <summary>
        /// Configure the input queue for receive bus
        /// </summary>
        /// <param name="configuration">Configuration to modify</param>
        /// <param name="sqsName">SQS queue name</param>
        /// <param name="region">Amazon Region the queue is in</param>
        /// <returns>Modified configuration</returns>
        public static IConfigureEventReceiving SetInputQueue(this IBusConfiguration configuration, string sqsName, RegionEndpoint region)
        {
            if (string.IsNullOrWhiteSpace(sqsName))
            {
                throw new JungleBusConfigurationException("sqsName", "Cannot have a blank input queue name");
            }

            if (region == null)
            {
                throw new JungleBusConfigurationException("region", "Region cannot be null");
            }

            if (configuration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Configuration cannot be null");
            }

            if (configuration.Receive == null)
            {
                configuration.Receive = new ReceiveConfiguration();
            }

            configuration.Receive.InputQueue = new SqsQueue(region, sqsName, new MessageParser(null));
            return configuration as IConfigureEventReceiving;
        }

        /// <summary>
        /// Configure the polling wait time for receive bus
        /// </summary>
        /// <param name="configuration">Configuration to modify</param>
        /// <param name="timeInSeconds">Number of seconds to the long polling to wait</param>
        /// <returns>Modified configuration</returns>
        public static IConfigureEventReceiving SetSqsPollWaitTime(this IConfigureEventReceiving configuration, int timeInSeconds)
        {
            if (timeInSeconds < 0 || timeInSeconds > 14)
            {
                throw new JungleBusConfigurationException("timeInSeconds", "Time in seconds must be between 0 and 14");
            }

            if (configuration.Receive == null)
            {
                throw new JungleBusConfigurationException("General", "Input queue needs to be configured before setting the wait timeout");
            }

            if (configuration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Configuration cannot be null");
            }

            SqsQueue queue = configuration.Receive.InputQueue as SqsQueue;
            if (queue == null)
            {
                throw new InvalidOperationException("Queue is not a SQS queue");
            }

            queue.WaitTimeSeconds = timeInSeconds;

            return configuration;
        }

        /// <summary>
        /// Configure the polling wait time for receive bus
        /// </summary>
        /// <param name="configuration">Configuration to modify</param>
        /// <param name="instances">Number of seconds to the long polling to wait</param>
        /// <returns>Modified configuration</returns>
        public static IConfigureEventReceiving SetNumberOfPollingInstances(this IConfigureEventReceiving configuration, int instances)
        {
            if (instances < 0)
            {
                throw new JungleBusConfigurationException("instances", "Time in seconds must be between 0 and 14");
            }

            if (configuration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Configuration cannot be null");
            }

            if (configuration.Receive == null)
            {
                throw new JungleBusConfigurationException("configuration", "Input queue needs to be configured before setting the wait timeout");
            }

            configuration.Receive.NumberOfPollingInstances = instances;
            return configuration;
        }

        /// <summary>
        /// Load the event handlers from the entry assembly
        /// </summary>
        /// <param name="configuration">Configuration to modify</param>
        /// <returns>Modified configuration</returns>
        public static IConfigureEventReceiving UsingEventHandlersFromEntryAssembly(this IConfigureEventReceiving configuration)
        {
            return configuration.UsingEventHandlers(Assembly.GetEntryAssembly().ExportedTypes);
        }

        /// <summary>
        /// Load the event handlers from the given types
        /// </summary>
        /// <param name="configuration">Configuration to modify</param>
        /// <param name="eventHandlers">Event handlers to register</param>
        /// <returns>Modified configuration</returns>
        public static IConfigureEventReceiving UsingEventHandlers(this IConfigureEventReceiving configuration, IEnumerable<Type> eventHandlers)
        {
            if (configuration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Configuration cannot be null");
            }

            if (configuration.ObjectBuilder == null)
            {
                throw new JungleBusConfigurationException("ObjectBuilder", "Object builder must be set");
            }

            if (configuration.Receive == null)
            {
                throw new JungleBusConfigurationException("Receive", "Input queue needs to be configured before setting event handlers");
            }

            configuration.Receive.Handlers = new Dictionary<Type, HashSet<Type>>();
            var handlerTypes = configuration.Receive.Handlers;
            var s_handlerTypes = eventHandlers.Where(x => x.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleMessage<>)));
            foreach (var handlerType in s_handlerTypes)
            {
                foreach (var handlerTypeType in handlerType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleMessage<>)))
                {
                    Type messageType = handlerTypeType.GenericTypeArguments[0];
                    if (!handlerTypes.ContainsKey(messageType))
                    {
                        handlerTypes[messageType] = new HashSet<Type>();
                    }

                    handlerTypes[messageType].Add(handlerType);
                }

                configuration.ObjectBuilder.RegisterType(handlerType, handlerType);
            }

            configuration.Receive.InputQueue.Subscribe(handlerTypes.Keys);

            return configuration;
        }
    }
}
