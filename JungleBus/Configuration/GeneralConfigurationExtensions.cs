using System;
using JungleBus.Exceptions;
using JungleBus.Interfaces;
using JungleBus.Interfaces.Serialization;
using JungleBus.IoC;
using JungleBus.Messaging;
using JungleBus.Serialization;
using StructureMap;

namespace JungleBus.Configuration
{
    /// <summary>
    /// Extension Methods for configuration the bus
    /// </summary>
    public static class GeneralConfigurationExtensions
    {
        /// <summary>
        /// Configure the the bus to use structure map to build the handlers
        /// </summary>
        /// <param name="configuration">Configuration to modify</param>
        /// <returns>Modified configuration</returns>
        public static IBusConfiguration WithStructureMapObjectBuilder(this IConfigureObjectBuilder configuration)
        {
            if (configuration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Configuration cannot be null");
            }

            configuration.ObjectBuilder = new StructureMapObjectBuilder();
            return configuration as IBusConfiguration;
        }

        /// <summary>
        /// Configure the the bus to use structure map to build the handlers with the given container
        /// </summary>
        /// <param name="configuration">Configuration to modify</param>
        /// <param name="container">Structure Map container to use</param>
        /// <returns>Modified configuration</returns>
        public static IBusConfiguration WithStructureMapObjectBuilder(this IConfigureObjectBuilder configuration, IContainer container)
        {
            if (configuration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Configuration cannot be null");
            }

            configuration.ObjectBuilder = new StructureMapObjectBuilder(container);
            return configuration as IBusConfiguration;
        }

        /// <summary>
        /// Configure the the bus to use JSON serialization of the messages
        /// </summary>
        /// <param name="configuration">Configuration to modify</param>
        /// <returns>Modified configuration</returns>
        public static IBusConfiguration UsingJsonSerialization(this IBusConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Configuration cannot be null");
            }

            if (configuration.ObjectBuilder == null)
            {
                throw new JungleBusConfigurationException("ObjectBuilder", "Object builder must be set");
            }

            configuration.ObjectBuilder.RegisterInstance<IMessageSerializer>(new JsonNetSerializer());
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

            if (configuration.Receive == null)
            {
                throw new JungleBusConfigurationException("Receive", "Receive has not been configured for this bus");
            }

            if (configuration.Receive != null)
            {
                configuration.ObjectBuilder.RegisterType(typeof(IMessageParser), typeof(MessageParser));
                configuration.Receive.InputQueue.MessageParser = configuration.ObjectBuilder.GetValue<IMessageParser>();
            }

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
