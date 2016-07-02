using System;
using System.Collections.Generic;
using Amazon;
using JungleBus.Aws.Sns;
using JungleBus.Exceptions;
using JungleBus.Messaging;
using JungleBus.Serialization;

namespace JungleBus.Aws
{
    /// <summary>
    /// Controls the actual bus
    /// </summary>
    public class AwsMessagePublisher : IMessagePublisher
    {
        /// <summary>
        /// SNS Client to publish to
        /// </summary>
        private readonly ISnsClient _snsClient;

        /// <summary>
        /// Message serializer
        /// </summary>
        private readonly IMessageSerializer _messageSerializer = new JsonNetSerializer();

        /// <summary>
        /// Initializes a new instance of the <see cref="AwsMessagePublisher" /> class.
        /// </summary>
        /// <param name="snsClient">SNS client to publish to</param>
        public AwsMessagePublisher(ISnsClient snsClient)
        {
            _snsClient = snsClient;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AwsMessagePublisher" /> class.
        /// </summary>
        /// <param name="endpoint">Amazon Endpoint we are connecting to</param>
        public AwsMessagePublisher(RegionEndpoint endpoint)
        {
            _snsClient = new SnsClient(endpoint);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AwsMessagePublisher" /> class.
        /// </summary>
        public AwsMessagePublisher()
        {
            _snsClient = null;
        }

        /// <summary>
        /// Publishes the serialized message
        /// </summary>
        /// <param name="message">Serialized Message</param>
        /// <param name="type">Payload type</param>
        public void Publish(string message, Type type)
        {
            if (_snsClient == null)
            {
                throw new JungleBusException("Public publishing is disabled");
            }

            _snsClient.Publish(message, type);
        }

        /// <summary>
        /// Sends a message to the given queue
        /// </summary>
        /// <param name="messageString">Message to publish</param>
        /// <param name="type">Type of message to send</param>
        /// <param name="localMessageQueue">Queue to send to</param>
        public void Send(string messageString, Type type, IMessageQueue localMessageQueue)
        {
            SnsMessage fakeMessage = new SnsMessage()
            {
                Message = messageString,
                MessageAttributes = new Dictionary<string, MessageAttribute>()
                {
                    { "messageType", new MessageAttribute() { Type = "String", Value = type.AssemblyQualifiedName } },
                },
            };

            localMessageQueue.AddMessage(_messageSerializer.Serialize(fakeMessage));
        }

        /// <summary>
        /// Setups the bus for publishing the given message types
        /// </summary>
        /// <param name="messageTypes">Message types</param>
        public void SetupMessagesForPublishing(IEnumerable<Type> messageTypes)
        {
            if (_snsClient == null)
            {
                throw new JungleBusException("Public publishing is disabled");
            }

            _snsClient.SetupMessagesForPublishing(messageTypes);
        }
    }
}
