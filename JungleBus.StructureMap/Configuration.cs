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

using JungleBus.Interfaces.Configuration;
using JungleBus.Interfaces.Exceptions;
using StructureMap;

namespace JungleBus.StructureMap
{
    public static class Configuration
    {
        /// <summary>
        /// Configure the the bus to use structure map to build the handlers
        /// </summary>
        /// <param name="configuration">Configuration to modify</param>
        /// <returns>Modified configuration</returns>
        public static IConfigureMessageSerializer WithStructureMapObjectBuilder(this IConfigureObjectBuilder configuration)
        {
            if (configuration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Configuration cannot be null");
            }

            configuration.ObjectBuilder = new StructureMapObjectBuilder();
            return configuration as IConfigureMessageSerializer;
        }

        /// <summary>
        /// Configure the the bus to use structure map to build the handlers with the given container
        /// </summary>
        /// <param name="configuration">Configuration to modify</param>
        /// <param name="container">Structure Map container to use</param>
        /// <returns>Modified configuration</returns>
        public static IConfigureMessageSerializer WithStructureMapObjectBuilder(this IConfigureObjectBuilder configuration, IContainer container)
        {
            if (configuration == null)
            {
                throw new JungleBusConfigurationException("configuration", "Configuration cannot be null");
            }

            configuration.ObjectBuilder = new StructureMapObjectBuilder(container);
            return configuration as IConfigureMessageSerializer;
        }
    }
}
