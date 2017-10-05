// <copyright file="GeneralConfigurationExtensions.cs">
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
using JungleBus.Interfaces;
using JungleBus.Interfaces.Configuration;
using JungleBus.Interfaces.Exceptions;
using JungleQueue.Interfaces.IoC;
using JungleQueue.Messaging;

namespace JungleBus.Configuration
{
    /// <summary>
    /// Extension Methods for configuration the bus
    /// </summary>
    public static class GeneralConfigurationExtensions
    {
        /// <summary>
        /// Configure the bus to use structure map to build the handlers
        /// </summary>
        /// <param name="configuration">Configuration to modify</param>
        /// <param name="objectBuilder">Object Builder to use</param>
        /// <returns>Modified configuration</returns>
        public static IConfigureMessageSerializer WithObjectBuilder(this IConfigureObjectBuilder configuration, IObjectBuilder objectBuilder)
        {
            if (configuration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Configuration cannot be null");
            }

            if (objectBuilder == null)
            {
                throw new JungleBusConfigurationException("objectBuilder", "ObjectBuilder cannot be null");
            }

            configuration.ObjectBuilder = objectBuilder;
            return configuration as IConfigureMessageSerializer;
        }

        /// <summary>
        /// Configure the bus to use JSON serialization of the messages
        /// </summary>
        /// <param name="configuration">Configuration to modify</param>
        /// <returns>Modified configuration</returns>
        public static IBusConfiguration UsingJsonSerialization(this IConfigureMessageSerializer configuration)
        {
            if (configuration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Configuration cannot be null");
            }

            if (configuration.ObjectBuilder == null)
            {
                throw new JungleBusConfigurationException("ObjectBuilder", "Object builder must be set");
            }

            // ToDo: Currently not being used, should reintroduce the message serializer in the next version
            // configuration.ObjectBuilder.RegisterInstance<IMessageSerializer>(new JsonNetSerializer());
            
            return configuration as IBusConfiguration;
        }

        /// <summary>
        /// Configure the bus to use log inbound and outbound messages
        /// </summary>
        /// <param name="configuration">Configuration to modify</param>
        /// <returns>Modified configuration</returns>
        public static IBusConfiguration EnableMessageLogging(this IBusConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Configuration cannot be null");
            }

            configuration.MessageLogger = new MessageLogger();
            return configuration;
        }

        /// <summary>
        /// Construct the bus
        /// </summary>
        /// <param name="configuration">Configuration to build from</param>
        /// <returns>Created bus</returns>
        public static IRunJungleBus CreateStartableBus(this IBusConfiguration configuration)
        {
            configuration.RunGeneralConfigurationValidation();

            if (configuration.InputQueueConfiguration == null)
            {
                throw new JungleBusConfigurationException("Receive", "Receive has not been configured for this bus");
            }

            configuration.InputQueueConfiguration.MessageLogger = configuration.MessageLogger;
            configuration.InputQueueConfiguration.ObjectBuilder = configuration.ObjectBuilder;

            JungleBus jungleBus = new JungleBus(configuration);
            return jungleBus;
        }

        /// <summary>
        /// Creates a send only bus factory from the configuration
        /// </summary>
        /// <param name="configuration">Configuration to build from</param>
        /// <returns>Factory for building send only buses</returns>
        public static Func<IBus> CreateSendOnlyBusFactory(this IBusConfiguration configuration)
        {
            configuration.RunGeneralConfigurationValidation();

            if (configuration.Send == null)
            {
                throw new JungleBusConfigurationException("Send", "Sending has not been configured for this bus");
            }

            JungleBus jungleBus = new JungleBus(configuration);
            return () => jungleBus.CreateSendBus();
        }

        /// <summary>
        /// Runs validation general to building a bus
        /// </summary>
        /// <param name="configuration">Configuration to validate</param>
        private static void RunGeneralConfigurationValidation(this IBusConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Configuration cannot be null");
            }

            if (configuration.ObjectBuilder == null)
            {
                throw new JungleBusConfigurationException("ObjectBuilder", "Object builder has not been configured");
            }
        }
    }
}
