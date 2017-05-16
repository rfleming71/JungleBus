// <copyright file="SqsQueue.cs">
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
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Amazon.SQS.Model;
using JungleBus.Interfaces.Exceptions;
using JungleBus.Queue.Messaging;

namespace JungleBus.Aws.Sqs
{
    /// <summary>
    /// Represents the SQS queue in Amazon AWS
    /// </summary>
    public sealed class SqsQueue : ISqsQueue, IDisposable
    {
        /// <summary>
        /// URL for the underlying queue
        /// </summary>
        private readonly string _queueUrl;

        /// <summary>
        /// ARN for the underlying queue
        /// </summary>
        private readonly string _queueArn;

        /// <summary>
        /// Instance of the SQS service
        /// </summary>
        private IAmazonSQS _simpleQueueService;

        /// <summary>
        /// Instance of the SNS service
        /// </summary>
        private Lazy<IAmazonSimpleNotificationService> _simpleNotificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqsQueue" /> class.
        /// </summary>
        /// <param name="endpoint">Region the queue is in</param>
        /// <param name="queueName">Name of the queue</param>
        /// <param name="retryCount">Number of times to retry a message before moving it to the dead letter queue</param>
        public SqsQueue(RegionEndpoint endpoint, string queueName, int retryCount)
        {
            _simpleQueueService = new AmazonSQSClient(endpoint);
            _simpleNotificationService = new Lazy<IAmazonSimpleNotificationService>(() => new AmazonSimpleNotificationServiceClient(endpoint));
            CreateQueueResponse createResponse = _simpleQueueService.CreateQueue(queueName);
            _queueUrl = createResponse.QueueUrl;
            var attributes = _simpleQueueService.GetAttributes(_queueUrl);
            _queueArn = attributes["QueueArn"];
            if (!attributes.ContainsKey("RedrivePolicy"))
            {
                createResponse = _simpleQueueService.CreateQueue(queueName + "_Dead_Letter");
                string deadLetterQueue = createResponse.QueueUrl;
                var deadLetterAttributes = _simpleQueueService.GetAttributes(deadLetterQueue);
                string redrivePolicy = string.Format(CultureInfo.InvariantCulture, "{{\"maxReceiveCount\":\"{0}\", \"deadLetterTargetArn\":\"{1}\" }}", retryCount, deadLetterAttributes["QueueArn"]);
                _simpleQueueService.SetQueueAttributes(_queueUrl, new Dictionary<string, string>() { { "RedrivePolicy", redrivePolicy }, { "MessageRetentionPeriod", "1209600" } });
                _simpleQueueService.SetQueueAttributes(createResponse.QueueUrl, new Dictionary<string, string>() { { "MessageRetentionPeriod", "1209600" } });
            }

            MessageParser = new MessageParser();
            MaxNumberOfMessages = 10;
        }

        /// <summary>
        /// Gets or sets the number of seconds to long poll for
        /// </summary>
        public int WaitTimeSeconds { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of messages the request can retrieve
        /// </summary>
        public int MaxNumberOfMessages { get; set; }

        /// <summary>
        /// Gets or sets the message parser for inbound messages
        /// </summary>
        public IMessageParser MessageParser { get; set; }

        /// <summary>
        /// Retrieve messages from the underlying queue
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Messages or empty</returns>
        public async Task<IEnumerable<TransportMessage>> GetMessages(CancellationToken cancellationToken)
        {
            ReceiveMessageRequest receiveMessageRequest = new ReceiveMessageRequest();
            receiveMessageRequest.WaitTimeSeconds = WaitTimeSeconds;
            receiveMessageRequest.MaxNumberOfMessages = MaxNumberOfMessages;
            receiveMessageRequest.AttributeNames = new List<string>() { "ApproximateReceiveCount" };
            receiveMessageRequest.MessageAttributeNames = new List<string>() { "messageType", "fromSns" };
            receiveMessageRequest.QueueUrl = _queueUrl;

            ReceiveMessageResponse receiveMessageResponse = await _simpleQueueService.ReceiveMessageAsync(receiveMessageRequest, cancellationToken);
            return receiveMessageResponse.Messages.Select(x => MessageParser.ParseMessage(x)).ToList();
        }

        /// <summary>
        /// Removes a message from the queue
        /// </summary>
        /// <param name="message">Message to remove</param>
        public void RemoveMessage(TransportMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (string.IsNullOrWhiteSpace(message.ReceiptHandle))
            {
                throw new JungleBusException("Invalid receipt handle");
            }

            _simpleQueueService.DeleteMessage(_queueUrl, message.ReceiptHandle);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, 
        /// or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_simpleQueueService != null)
            {
                _simpleQueueService.Dispose();
                _simpleQueueService = null;
            }
        }

        /// <summary>
        /// Subscribe the queue to the given SNS topics
        /// </summary>
        /// <param name="snsTopics">Message topics to subscribe to</param>
        public void Subscribe(IEnumerable<string> snsTopics)
        {
            foreach (string topicName in snsTopics)
            {
                var topic = _simpleNotificationService.Value.FindTopic(topicName);
                if (topic != null)
                {
                    string arn = _simpleNotificationService.Value.SubscribeQueue(topic.TopicArn, _simpleQueueService, _queueUrl);
                    _simpleNotificationService.Value.SetSubscriptionAttributes(arn, "RawMessageDelivery", "true");
                }
            }
        }

        /// <summary>
        /// Adds the message to the queue
        /// </summary>
        /// <param name="message">Message to add to the queue</param>
        /// <param name="metadata">Message metadata</param>
        public void AddMessage(string message, IEnumerable<KeyValuePair<string, string>> metadata)
        {
            SendMessageRequest request = new SendMessageRequest(_queueUrl, message);
            foreach (KeyValuePair<string, string> kvp in metadata)
            {
                request.MessageAttributes[kvp.Key] = new MessageAttributeValue() { StringValue = kvp.Value, DataType = "String" };
            }

            _simpleQueueService.SendMessage(request);
        }
    }
}
