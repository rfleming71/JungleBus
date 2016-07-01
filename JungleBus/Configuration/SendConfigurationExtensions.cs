using System;
using System.Collections.Generic;
using System.Linq;
using Amazon;
using JungleBus.Aws;
using JungleBus.Exceptions;

namespace JungleBus.Configuration
{
    /// <summary>
    /// Extension Methods for configuration the bus
    /// </summary>
    public static class SendConfigurationExtensions
    {
        /// <summary>
        /// Configure the input queue for send bus
        /// </summary>
        /// <param name="configuration">Configuration to modify</param>
        /// <param name="messageTypes">Messages that can be published</param>
        /// <param name="region">Amazon Region the topic is in</param>
        /// <returns>Modified configuration</returns>
        public static IConfigureEventPublishing PublishingMessages(this IBusConfiguration configuration, IEnumerable<Type> messageTypes, RegionEndpoint region)
        {
            if (messageTypes == null || !messageTypes.Any())
            {
                throw new JungleBusConfigurationException("messageTypes", "Cannot have a blank publish queue name");
            }

            if (region == null)
            {
                throw new JungleBusConfigurationException("region", "Region cannot be null");
            }

            if (configuration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Configuration cannot be null");
            }

            if (configuration.Send != null)
            {
                throw new JungleBusConfigurationException("PublishingMessages", "PublishingMessages is already configured");
            }

            configuration.Send = new SendConfiguration();
            configuration.Send.MessagePublisher = new AwsMessagePublisher(region);
            configuration.Send.MessagePublisher.SetupMessagesForPublishing(messageTypes);

            return configuration as IConfigureEventPublishing;
        }

        /// <summary>
        /// Configures the bus to allow local message sending only
        /// </summary>
        /// <param name="configuration">Configuration to modify</param>
        /// <param name="region"></param>
        /// <returns>Modified configuration</returns>
        public static IBusConfiguration PublishingLocalEventsOnly(this IBusConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Configuration cannot be null");
            }

            if (configuration.Send != null)
            {
                throw new JungleBusConfigurationException("PublishingMessages", "PublishingMessages is already configured");
            }

            configuration.Send = new SendConfiguration();
            configuration.Send.MessagePublisher = new AwsMessagePublisher();
            return configuration as IConfigureEventPublishing;
        }
    }
}
