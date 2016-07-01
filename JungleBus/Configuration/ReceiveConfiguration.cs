using System;
using System.Collections.Generic;
using JungleBus.Messaging;

namespace JungleBus.Configuration
{
    /// <summary>
    /// Configuration information for the receive bus
    /// </summary>
    public class ReceiveConfiguration
    {
        /// <summary>
        /// Gets or sets the queue to receive events on
        /// </summary>
        public IMessageQueue InputQueue { get; set; }

        /// <summary>
        /// Gets or sets the number of polling instances to run
        /// </summary>
        public int NumberOfPollingInstances { get; set; }

        /// <summary>
        /// Gets or sets the collection of assemblies to scan for message handlers
        /// </summary>
        internal Dictionary<Type, HashSet<Type>> Handlers { get; set; }
    }
}
