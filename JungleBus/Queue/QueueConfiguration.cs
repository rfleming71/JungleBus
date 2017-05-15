﻿using System;
using System.Collections.Generic;
using Amazon;

namespace JungleBus.Queue
{
    /// <summary>
    /// Queue configuration object
    /// </summary>
    public class QueueConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the input queue
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// Gets or sets the AWS Region
        /// </summary>
        public RegionEndpoint Region { get; set; }

        /// <summary>
        /// Gets or sets the number of times to attempt to process a message
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Gets or sets the number of polling instances to run
        /// </summary>
        public int NumberOfPollingInstances { get; set; }

        /// <summary>
        /// Gets or sets the collection of message handlers organized by message type
        /// </summary>
        public Dictionary<Type, HashSet<Type>> Handlers { get; set; }

        /// <summary>
        /// Gets or sets the collection of message fault handlers organized by message type
        /// </summary>
        public Dictionary<Type, HashSet<Type>> FaultHandlers { get; set; }
    }
}
