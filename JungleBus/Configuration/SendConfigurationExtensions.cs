// <copyright file="SendConfigurationExtensions.cs">
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
            configuration.Send.MessagePublisher = new AwsMessagePublisher(region, configuration.MessageLogger);
            configuration.Send.MessagePublisher.SetupMessagesForPublishing(messageTypes);

            return configuration as IConfigureEventPublishing;
        }

        /// <summary>
        /// Configures the bus to allow local message sending only
        /// </summary>
        /// <param name="configuration">Configuration to modify</param>
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
            configuration.Send.MessagePublisher = new AwsMessagePublisher(configuration.MessageLogger);
            return configuration as IConfigureEventPublishing;
        }
    }
}
