﻿using System.Collections.Generic;
using Amazon.SQS.Model;
using JungleBus.Queue.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JungleBus.Tests.Messaging
{
    [TestClass]
    public class MessageParserTests
    {
        private MessageParser _parser;
        private Dictionary<string, MessageAttributeValue> _messageAttributes;

        [TestInitialize]
        public void TestInitialize()
        {
            _messageAttributes = new Dictionary<string, MessageAttributeValue>() { { "messageType", new MessageAttributeValue() { DataType = "String", StringValue = typeof(TestMessage).AssemblyQualifiedName } } };
            _parser = new MessageParser();
        }

        [TestMethod]
        public void MessageParserTests_success()
        {
            Message sourceMessage = new Message() { ReceiptHandle = "12345", Body = "{ }", MessageAttributes = _messageAttributes };
            TransportMessage transportMessage = _parser.ParseMessage(sourceMessage);
            Assert.IsNotNull(transportMessage);
            Assert.IsTrue(transportMessage.MessageParsingSucceeded);
            Assert.IsNotNull(transportMessage.Message);
            Assert.AreEqual(typeof(TestMessage), transportMessage.MessageType);
            Assert.AreEqual(typeof(TestMessage).AssemblyQualifiedName, transportMessage.MessageTypeName);
            Assert.AreEqual("12345", transportMessage.ReceiptHandle);
            Assert.AreEqual("{ }", transportMessage.Body);
        }

        [TestMethod]
        public void MessageParserTests_Unknown_Type()
        {
            _messageAttributes["messageType"].StringValue = "Invalid Type";
            Message sourceMessage = new Message() { ReceiptHandle = "12345", Body = "{}", MessageAttributes = _messageAttributes };
            TransportMessage transportMessage = _parser.ParseMessage(sourceMessage);
            Assert.IsNotNull(transportMessage);
            Assert.IsFalse(transportMessage.MessageParsingSucceeded);
            Assert.IsNull(transportMessage.Message);
            Assert.IsNull(transportMessage.MessageType);
            Assert.AreEqual("Invalid Type", transportMessage.MessageTypeName);
            Assert.AreEqual("12345", transportMessage.ReceiptHandle);
            Assert.AreEqual("{}", transportMessage.Body);
        }

        [TestMethod]
        public void MessageParserTests_Deserialization_failure_message_body()
        {
            Message sourceMessage = new Message() { ReceiptHandle = "12345", Body = "sdsdsd", MessageAttributes = _messageAttributes };
            TransportMessage transportMessage = _parser.ParseMessage(sourceMessage);
            Assert.IsNotNull(transportMessage);
            Assert.IsFalse(transportMessage.MessageParsingSucceeded);
            Assert.IsNull(transportMessage.Message);
            Assert.AreEqual(typeof(TestMessage), transportMessage.MessageType);
            Assert.AreEqual(typeof(TestMessage).AssemblyQualifiedName, transportMessage.MessageTypeName);
            Assert.AreEqual("12345", transportMessage.ReceiptHandle);
            Assert.AreEqual("sdsdsd", transportMessage.Body);
        }
    }
}
