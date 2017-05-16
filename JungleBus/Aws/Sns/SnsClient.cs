// <copyright file="SnsClient.cs">
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
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace JungleBus.Aws.Sns
{
    /// <summary>
    /// Client for talking to SNS
    /// </summary>
    public sealed class SnsClient : IDisposable, ISnsClient
    {
        /// <summary>
        /// Cached list of topic ARNs
        /// </summary>
        private readonly Dictionary<string, string> _topicArns;

        /// <summary>
        /// Function to build the names of the topics
        /// </summary>
        private readonly Func<Type, string> _topicFormatter;

        /// <summary>
        /// Connection to SNS
        /// </summary>
        private IAmazonSimpleNotificationService _sns;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnsClient" /> class.
        /// </summary>
        /// <param name="endpoint">Amazon Endpoint we are connecting to</param>
        /// <param name="topicFormatter">Function to build the names of the topics</param>
        public SnsClient(RegionEndpoint endpoint, Func<Type, string> topicFormatter)
        {
            if (topicFormatter == null)
            {
                throw new ArgumentNullException("topicFormatter");
            }

            _sns = new AmazonSimpleNotificationServiceClient(endpoint);
            _topicArns = new Dictionary<string, string>();
            _topicFormatter = topicFormatter;
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
        /// <param name="metadata">Message metadata</param>
        public void Publish(string message, Type type, Dictionary<string, string> metadata)
        {
            string topicName = _topicFormatter(type);
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
            if (metadata != null)
            {
                foreach (var md in metadata)
                {
                    request.MessageAttributes[md.Key] = new MessageAttributeValue() { StringValue = md.Value, DataType = "String" };
                }
            }

            _sns.Publish(request);
        }

        /// <summary>
        /// Setups the bus for publishing the given message types
        /// </summary>
        /// <param name="messageTypes">Message types</param>
        public void SetupMessagesForPublishing(IEnumerable<Type> messageTypes)
        {
            foreach (Type messageType in messageTypes)
            {
                string topicName = _topicFormatter(messageType);
                if (!_topicArns.ContainsKey(topicName))
                {
                    Topic topic = _sns.FindTopic(topicName);
                    if (topic == null)
                    {
                        _topicArns[topicName] = CreateTopic(topicName);
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
        private string CreateTopic(string topicName)
        {
            CreateTopicResponse response = _sns.CreateTopic(topicName);
            return response.TopicArn;
        }
    }
}
