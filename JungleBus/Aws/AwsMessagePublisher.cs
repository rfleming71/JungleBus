using System;
using System.Collections.Generic;
using Amazon;
using JungleBus.Aws.Sns;
using JungleBus.Exceptions;
using JungleBus.Messaging;
using JungleBus.Serialization;

namespace JungleBus.Aws
{
    public class AwsMessagePublisher : IMessagePublisher
    {
        private readonly SnsClient _snsClient;
        private readonly IMessageSerializer _messageSerializer;

        public AwsMessagePublisher(RegionEndpoint endpoint)
        {
            _snsClient = new SnsClient(endpoint);
            _messageSerializer = new JsonNetSerializer();
        }

        public AwsMessagePublisher()
        {
            _snsClient = null;
            _messageSerializer = new JsonNetSerializer();
        }

        public void Publish(string message, Type type)
        {
            if (_snsClient == null)
            {
                throw new JungleBusException("Public publishing is disabled");
            }

            _snsClient.Publish(message, type);
        }

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
