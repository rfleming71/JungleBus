using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using JungleBus.Messaging;

namespace JungleBus.Aws.Sns
{ 
    /// <summary>
    /// Client for talking to SNS
    /// </summary>
    public sealed class SnsClient : IDisposable
    {
        /// <summary>
        /// Cached list of topic ARNs
        /// </summary>
        private readonly Dictionary<string, string> _topicArns;

        /// <summary>
        /// Connection to SNS
        /// </summary>
        private IAmazonSimpleNotificationService _sns;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnsClient" /> class.
        /// </summary>
        /// <param name="endpoint">Amazon Endpoint we are connecting to</param>
        public SnsClient(RegionEndpoint endpoint)
        {
            _sns = new AmazonSimpleNotificationServiceClient(endpoint);
            _topicArns = new Dictionary<string, string>();
        }

        /// <summary>
        /// Format a message type into a topic name
        /// </summary>
        /// <param name="messageType">Message type</param>
        /// <returns>Formatted Topic name</returns>
        public static string GetTopicName(Type messageType)
        {
            if (messageType == null)
            {
                throw new ArgumentNullException("messageType");
            }

            return messageType.FullName.Replace('.', '_');
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, 
        /// or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_sns != null)
            {
                _sns.Dispose();
                _sns = null;
            }
        }

        /// <summary>
        /// Publishes the serialized message
        /// </summary>
        /// <param name="message">Serialized Message</param>
        /// <param name="type">Payload type</param>
        public async void Publish(string message, Type type)
        {
            string topicName = GetTopicName(type);
            if (!_topicArns.ContainsKey(topicName))
            {
                Topic topic = _sns.FindTopic(topicName);
                if (topic == null)
                {
                    throw new InvalidParameterException("Unknown topic name " + topicName);
                }
                else
                {
                    _topicArns[topicName] = topic.TopicArn;
                }
            }

            PublishRequest request = new PublishRequest(_topicArns[topicName], message);
            request.MessageAttributes["messageType"] = new MessageAttributeValue() { StringValue = type.AssemblyQualifiedName, DataType = "String" };
            request.MessageAttributes["fromSns"] = new MessageAttributeValue() { StringValue = "True", DataType = "String" };

            await _sns.PublishAsync(request);
        }

        /// <summary>
        /// Setups the bus for publishing the given message types
        /// </summary>
        /// <param name="messageTypes">Message types</param>
        public async void SetupMessagesForPublishing(IEnumerable<Type> messageTypes)
        {
            foreach (Type messageType in messageTypes)
            {
                string topicName = GetTopicName(messageType);
                if (!_topicArns.ContainsKey(topicName))
                {
                    Topic topic = _sns.FindTopic(topicName);
                    if (topic == null)
                    {
                        _topicArns[topicName] = await CreateTopic(topicName);
                    }
                    else
                    {
                        _topicArns[topicName] = topic.TopicArn;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a topic with the given name
        /// </summary>
        /// <param name="topicName">Topic name</param>
        /// <returns>Created topic ARN</returns>
        private async Task<string> CreateTopic(string topicName)
        {
            CreateTopicResponse response = await _sns.CreateTopicAsync(topicName);
            return response.TopicArn;
        }

        public void Send(string messageString, Type type, IMessageQueue _localMessageQueue)
        {
            throw new NotImplementedException();
        }
    }
}
