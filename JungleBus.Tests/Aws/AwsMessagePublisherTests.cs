﻿using System;
using System.Collections.Generic;
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
            mockClient.Setup(x => x.Publish(It.IsAny<string>(), It.IsAny<Type>(), It.IsAny<Dictionary<string, string>>()));
            AwsMessagePublisher publisher = new AwsMessagePublisher(mockClient.Object, _mockLogger.Object);
            publisher.Publish("test message", typeof(TestMessage));
            mockClient.Verify(x => x.Publish("test message", typeof(TestMessage), It.IsAny<Dictionary<string, string>>()), Times.Once());
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
    }
}
