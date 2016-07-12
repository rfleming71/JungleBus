using System;
using JungleBus.Aws;
using JungleBus.Aws.Sns;
using JungleBus.Exceptions;
using JungleBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace JungleBus.Tests.Aws
{
    [TestClass]
    public class AwsMessagePublisherTests
    {
        private Mock<IMessageLogger> _mockLogger;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockLogger = new Mock<IMessageLogger>(MockBehavior.Strict);
            _mockLogger.Setup(x => x.OutboundLogMessage(It.IsAny<string>(), It.IsAny<string>()));
        }

        [TestMethod]
        public void AwsMessagePublisherTests_Publish()
        {
            Mock<ISnsClient> mockClient = new Mock<ISnsClient>(MockBehavior.Strict);
            mockClient.Setup(x => x.Publish(It.IsAny<string>(), It.IsAny<Type>()));
            AwsMessagePublisher publisher = new AwsMessagePublisher(mockClient.Object, _mockLogger.Object);
            publisher.Publish("test message", typeof(TestMessage));
            mockClient.Verify(x => x.Publish("test message", typeof(TestMessage)), Times.Once());
            _mockLogger.Verify(x => x.OutboundLogMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public void AwsMessagePublisherTests_Publish_NoPublicPublish_ThrowsException()
        {
            try
            {
                AwsMessagePublisher publisher = new AwsMessagePublisher(_mockLogger.Object);
                publisher.Publish("string", typeof(TestMessage));
                Assert.Fail();
            }
            catch (JungleBusException ex)
            {
                Assert.AreEqual("Public publishing is disabled", ex.Message);
            }
        }

        [TestMethod]
        public void AwsMessagePublisherTests_SetupMessagesForPublishing_NoPublicPublish_ThrowsException()
        {
            try
            {
                AwsMessagePublisher publisher = new AwsMessagePublisher(_mockLogger.Object);
                publisher.SetupMessagesForPublishing(new Type[] { typeof(TestMessage) });
                Assert.Fail();
            }
            catch (JungleBusException ex)
            {
                Assert.AreEqual("Public publishing is disabled", ex.Message);
            }
        }

        [TestMethod]
        public void AwsMessagePublisherTests_Send()
        {
            Mock<IMessageQueue> mockQueue = new Mock<IMessageQueue>(MockBehavior.Strict);
            string sentMessage = null;
            mockQueue.Setup(x => x.AddMessage(It.IsAny<string>())).Callback<string>(x => sentMessage = x);

            AwsMessagePublisher publisher = new AwsMessagePublisher(_mockLogger.Object);
            publisher.Send("message body", typeof(TestMessage), mockQueue.Object);
            mockQueue.Verify(x => x.AddMessage(It.IsAny<string>()), Times.Once());
            Assert.IsNotNull(sentMessage);
            SnsMessage message = JsonConvert.DeserializeObject<SnsMessage>(sentMessage);
            Assert.IsNotNull(message);
            Assert.AreEqual("message body", message.Message);
            Assert.AreEqual("String", message.MessageAttributes["messageType"].Type);
            Assert.AreEqual("JungleBus.Tests.TestMessage, JungleBus.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", message.MessageAttributes["messageType"].Value);
            _mockLogger.Verify(x => x.OutboundLogMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }
    }
}
