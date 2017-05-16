// <copyright file="ReceiveConfigurationExtensions.cs">
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
using System.Linq;
using System.Reflection;
using Amazon;
using JungleBus.Exceptions;
using JungleBus.Interfaces;
using JungleBus.Interfaces.IoC;
using JungleBus.Queue;

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

            configuration.InputQueueConfiguration = new QueueConfiguration();

            if (!string.IsNullOrWhiteSpace(configuration.BusName))
            {
                sqsName = string.Format("{0}_{1}", configuration.BusName, sqsName);
            }

            configuration.InputQueueConfiguration.QueueName = sqsName;
            configuration.InputQueueConfiguration.NumberOfPollingInstances = 1;
            configuration.InputQueueConfiguration.Region = region;
            configuration.InputQueueConfiguration.RetryCount = 5;
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

            if (configuration.InputQueueConfiguration == null)
            {
                throw new JungleBusConfigurationException("General", "Input queue needs to be configured before setting the wait timeout");
            }

            if (configuration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Configuration cannot be null");
            }

            configuration.InputQueueConfiguration.SqsPollWaitTime = timeInSeconds;
            return configuration;
        }

        /// <summary>
        /// Configure the number of polling instances to run for receive bus
        /// </summary>
        /// <param name="configuration">Configuration to modify</param>
        /// <param name="instances">Number of polling instances to run</param>
        /// <returns>Modified configuration</returns>
        public static IConfigureEventReceiving SetNumberOfPollingInstances(this IConfigureEventReceiving configuration, int instances)
        {
            if (instances < 0)
            {
                throw new JungleBusConfigurationException("instances", "Number of instances must be zero or greater");
            }

            if (configuration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Configuration cannot be null");
            }

            if (configuration.InputQueueConfiguration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Input queue needs to be configured before setting the number of polling instances");
            }

            configuration.InputQueueConfiguration.NumberOfPollingInstances = instances;
            return configuration;
        }

        /// <summary>
        /// Load the event handlers from the entry assembly
        /// </summary>
        /// <param name="configuration">Configuration to modify</param>
        /// <returns>Modified configuration</returns>
        public static IConfigureEventReceiving UsingEventHandlersFromEntryAssembly(this IConfigureEventReceiving configuration)
        {
            IEnumerable<Type> types = Assembly.GetEntryAssembly().ExportedTypes;
            return configuration
                .UsingEventHandlers(types)
                .UsingEventFaultHandlers(types);
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

            if (configuration.InputQueueConfiguration == null)
            {
                throw new JungleBusConfigurationException("Receive", "Input queue needs to be configured before setting event handlers");
            }

            configuration.InputQueueConfiguration.Handlers = ScanForTypes(eventHandlers, typeof(IHandleMessage<>), configuration.ObjectBuilder);
            configuration.InputQueueConfiguration.FaultHandlers = new Dictionary<Type, HashSet<Type>>();
            return configuration;
        }

        /// <summary>
        /// Load the event handlers from the given types
        /// </summary>
        /// <param name="configuration">Configuration to modify</param>
        /// <param name="eventFaultHandlers">Event fault handlers to register</param>
        /// <returns>Modified configuration</returns>
        public static IConfigureEventReceiving UsingEventFaultHandlers(this IConfigureEventReceiving configuration, IEnumerable<Type> eventFaultHandlers)
        {
            if (configuration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Configuration cannot be null");
            }

            if (configuration.ObjectBuilder == null)
            {
                throw new JungleBusConfigurationException("ObjectBuilder", "Object builder must be set");
            }

            if (configuration.InputQueueConfiguration == null)
            {
                throw new JungleBusConfigurationException("Receive", "Input queue needs to be configured before setting event fault handlers");
            }

            configuration.InputQueueConfiguration.FaultHandlers = ScanForTypes(eventFaultHandlers, typeof(IHandleMessageFaults<>), configuration.ObjectBuilder);
            return configuration;
        }

        /// <summary>
        /// Scans the given types for instances of the requested interface
        /// </summary>
        /// <param name="typesToScan">Types to scan</param>
        /// <param name="handlerTypeToFind">Handler type to find</param>
        /// <param name="objectBuilder">Object builder to register the types with</param>
        /// <returns>Types found</returns>
        private static Dictionary<Type, HashSet<Type>> ScanForTypes(IEnumerable<Type> typesToScan, Type handlerTypeToFind, IObjectBuilder objectBuilder)
        {
            Dictionary<Type, HashSet<Type>> results = new Dictionary<Type, HashSet<Type>>();
            var handlerTypes = typesToScan.Where(x => x.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerTypeToFind));
            foreach (var handlerType in handlerTypes)
            {
                foreach (var handlerTypeType in handlerType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerTypeToFind))
                {
                    Type messageType = handlerTypeType.GenericTypeArguments[0];
                    if (!results.ContainsKey(messageType))
                    {
                        results[messageType] = new HashSet<Type>();
                    }

                    results[messageType].Add(handlerType);
                }

                objectBuilder.RegisterType(handlerType, handlerType);
            }

            return results;
        }
    }
}