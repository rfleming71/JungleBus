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
using JungleBus.Aws.Sns;
using JungleBus.Exceptions;
using JungleBus.Messaging;

namespace JungleBus.Aws.Sqs
{
    /// <summary>
    /// Represents the SQS queue in Amazon AWS
    /// </summary>
    public sealed class SqsQueue : IMessageQueue, IDisposable
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
        /// <param name="messageParser">Message parser</param>
        public SqsQueue(RegionEndpoint endpoint, string queueName, int retryCount, IMessageParser messageParser)
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
                string redrivePolicy = string.Format(CultureInfo.InvariantCulture, "{{\"maxReceiveCount\":\"{0}\", \"deadLetterTargetArn\":\"{1}\"}}", retryCount, deadLetterAttributes["QueueArn"]);
                _simpleQueueService.SetQueueAttributes(_queueUrl, new Dictionary<string, string>() { { "RedrivePolicy", redrivePolicy } });
            }

            MessageParser = messageParser;
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
        /// Subscribe the queue to the given message types
        /// </summary>
        /// <param name="messageTypes">Message to subscribe to</param>
        public void Subscribe(IEnumerable<Type> messageTypes)
        {
            foreach (Type messageType in messageTypes)
            {
                var topic = _simpleNotificationService.Value.FindTopic(SnsClient.GetTopicName(messageType));
                if (topic != null)
                {
                    _simpleNotificationService.Value.SubscribeQueue(topic.TopicArn, _simpleQueueService, _queueUrl);
                }
            }
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

            if (_simpleNotificationService != null && _simpleNotificationService.IsValueCreated)
            {
                _simpleNotificationService.Value.Dispose();
                _simpleNotificationService = null;
            }
        }

        /// <summary>
        /// Adds the message to the queue
        /// </summary>
        /// <param name="message">Message to add to the queue</param>
        public void AddMessage(string message)
        {
            _simpleQueueService.SendMessage(_queueUrl, message);
        }
    }
}
