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
using System.Reflection;
using Amazon;
using JungleBus.Interfaces.Exceptions;
using JungleQueue.Configuration;

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
            
            if (!string.IsNullOrWhiteSpace(configuration.BusName))
            {
                sqsName = string.Format("{0}_{1}", configuration.BusName, sqsName);
            }

            configuration.InputQueueConfiguration = QueueBuilder
                .Create(sqsName, region)
                .WithObjectBuilder(configuration.ObjectBuilder)
                .UsingJsonSerialization()
                as QueueConfiguration;

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
            if (configuration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Configuration cannot be null");
            }

            configuration.InputQueueConfiguration.SetSqsPollWaitTime(timeInSeconds);
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
            if (configuration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Configuration cannot be null");
            }

            configuration.InputQueueConfiguration.WithMaxSimultaneousMessages(instances);
            return configuration;
        }

        /// <summary>
        /// Load the event handlers from the entry assembly
        /// </summary>
        /// <param name="configuration">Configuration to modify</param>
        /// <returns>Modified configuration</returns>
        public static IConfigureEventReceiving UsingEventHandlersFromEntryAssembly(this IConfigureEventReceiving configuration)
        {
            if (configuration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Configuration cannot be null");
            }

            if (configuration.InputQueueConfiguration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Input Configuration cannot be null");
            }

            IEnumerable<Type> types = Assembly.GetEntryAssembly().ExportedTypes;
            configuration.InputQueueConfiguration
                .UsingEventHandlers(types)
                .UsingEventFaultHandlers(types);

            return configuration;
        }
    }
}