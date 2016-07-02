using System;
using System.Collections.Generic;
using Amazon.SQS.Model;
using JungleBus.Messaging;
using JungleBus.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JungleBus.Tests.Messaging
{
    [TestClass]
    public class MessageParserTests
    {
        private MessageParser _parser;
        private Mock<IMessageSerializer> _serializer;
        private Dictionary<string, MessageAttribute> _messageAttributes;

        [TestInitialize]
        public void TestInitialize()
        {
            _messageAttributes = new Dictionary<string, MessageAttribute>() { { "messageType", new MessageAttribute() { Type = "String", Value = typeof(TestMessage).AssemblyQualifiedName } } };
            _serializer = new Mock<IMessageSerializer>(MockBehavior.Strict);
            _serializer.Setup(x => x.Deserialize(It.IsAny<string>(), typeof(SnsMessage)))
                .Returns(new SnsMessage()
                {
                    Message = "Body",
                    MessageAttributes = _messageAttributes
                });
            _serializer.Setup(x => x.Deserialize(It.IsAny<string>(), typeof(TestMessage)))
                .Returns(new TestMessage());
            _parser = new MessageParser(_serializer.Object);
        }

        [TestMethod]
        public void MessageParserTests_success()
        {
            Message sourceMessage = new Message() { ReceiptHandle = "12345", Body = "{ Body = {  }, MessageAttribute = {}}" };
            TransportMessage transportMessage = _parser.ParseMessage(sourceMessage);
            Assert.IsNotNull(transportMessage);
            Assert.IsTrue(transportMessage.MessageParsingSucceeded);
            Assert.IsNotNull(transportMessage.Message);
            Assert.AreEqual(typeof(TestMessage), transportMessage.MessageType);
            Assert.AreEqual(typeof(TestMessage).AssemblyQualifiedName, transportMessage.MessageTypeName);
            Assert.AreEqual("12345", transportMessage.ReceiptHandle);
            Assert.AreEqual("Body", transportMessage.Body);
        }

        [TestMethod]
        public void MessageParserTests_Unknown_Type()
        {
            _messageAttributes["messageType"].Value = "Invalid Type";
            Message sourceMessage = new Message() { ReceiptHandle = "12345", Body = "{ Body = {  }, MessageAttribute = {}}" };
            TransportMessage transportMessage = _parser.ParseMessage(sourceMessage);
            Assert.IsNotNull(transportMessage);
            Assert.IsFalse(transportMessage.MessageParsingSucceeded);
            Assert.IsNull(transportMessage.Message);
            Assert.IsNull(transportMessage.MessageType);
            Assert.AreEqual("Invalid Type", transportMessage.MessageTypeName);
            Assert.AreEqual("12345", transportMessage.ReceiptHandle);
            Assert.AreEqual("Body", transportMessage.Body);
        }

        [TestMethod]
        public void MessageParserTests_Deserialization_failure_message_body()
        {
            _serializer.Setup(x => x.Deserialize(It.IsAny<string>(), typeof(TestMessage))).Throws(new Exception());
            Message sourceMessage = new Message() { ReceiptHandle = "12345", Body = "{ Body = {  }, MessageAttribute = {}}" };
            TransportMessage transportMessage = _parser.ParseMessage(sourceMessage);
            Assert.IsNotNull(transportMessage);
            Assert.IsFalse(transportMessage.MessageParsingSucceeded);
            Assert.IsNull(transportMessage.Message);
            Assert.AreEqual(typeof(TestMessage), transportMessage.MessageType);
            Assert.AreEqual(typeof(TestMessage).AssemblyQualifiedName, transportMessage.MessageTypeName);
            Assert.AreEqual("12345", transportMessage.ReceiptHandle);
            Assert.AreEqual("Body", transportMessage.Body);
        }

        [TestMethod]
        public void MessageParserTests_Deserialization_failure_message_wrapper()
        {
            _serializer.Setup(x => x.Deserialize(It.IsAny<string>(), typeof(SnsMessage))).Throws(new Exception());
            Message sourceMessage = new Message() { ReceiptHandle = "12345", Body = "{ Body = {  }, MessageAttribute = {}}" };
            TransportMessage transportMessage = _parser.ParseMessage(sourceMessage);
            Assert.IsNotNull(transportMessage);
            Assert.IsFalse(transportMessage.MessageParsingSucceeded);
            Assert.IsNull(transportMessage.Message);
            Assert.IsNull(transportMessage.MessageType);
            Assert.IsNull(transportMessage.MessageTypeName);
            Assert.AreEqual("12345", transportMessage.ReceiptHandle);
            Assert.IsNull(transportMessage.Body);
        }
    }
}
