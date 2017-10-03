﻿// <copyright file="BusBuilder.cs">
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
using JungleBus.Interfaces.Configuration;

namespace JungleBus.Configuration
{
    /// <summary>
    /// Starting class for configuring the AWS Bus
    /// </summary>
    public static class BusBuilder
    {
        /// <summary>
        /// Create the default bus configuration
        /// </summary>
        /// <returns>Default bus configuration</returns>
        public static IConfigureObjectBuilder Create()
        {
            return Create(null);
        }

        /// <summary>
        /// Create the default bus configuration
        /// </summary>
        /// <param name="busName">Name of the bus to create</param>
        /// <returns>Default bus configuration</returns>
        public static IConfigureObjectBuilder Create(string busName)
        {
            IBusConfiguration configuration = new BusConfiguration()
            {
                MessageLogger = new JungleQueue.Messaging.NoOpMessageLogger(),
                BusName = busName,
            };

            configuration.SubscriptionFormatter = GetDefaultFormatter(configuration);

            return configuration as IConfigureObjectBuilder;
        }

        /// <summary>
        /// Creates the default topic name formatter
        /// </summary>
        /// <param name="configuration">Bus configuration</param>
        /// <returns>Topic Formatter</returns>
        private static Func<Type, string> GetDefaultFormatter(IBusConfiguration configuration)
        {
            return (Type messageType) =>
            {
                if (messageType == null)
                {
                    throw new ArgumentNullException("messageType");
                }

                string name = messageType.FullName.Replace('.', '_');

                if (!string.IsNullOrWhiteSpace(configuration.BusName))
                {
                    name = string.Format("{0}_{1}", configuration.BusName, name);
                }

                return name;
            };
        }
    }
}
