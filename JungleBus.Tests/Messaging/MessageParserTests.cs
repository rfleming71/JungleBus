using System.Collections.Generic;
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
        private Dictionary<string, string> _attributes;

        [TestInitialize]
        public void TestInitialize()
        {
            _attributes = new Dictionary<string, string>() { { "ApproximateReceiveCount", "3" } };
            _messageAttributes = new Dictionary<string, MessageAttributeValue>() { { "messageType", new MessageAttributeValue() { DataType = "String", StringValue = typeof(TestMessage).AssemblyQualifiedName } } };
            _parser = new MessageParser();
        }

        [TestMethod]
        public void MessageParserTests_success()
        {
            Message sourceMessage = new Message() { ReceiptHandle = "12345", Body = "{ }", MessageAttributes = _messageAttributes, Attributes = _attributes };
            TransportMessage transportMessage = _parser.ParseMessage(sourceMessage);
            Assert.IsNotNull(transportMessage);
            Assert.IsTrue(transportMessage.MessageParsingSucceeded);
            Assert.IsFalse(transportMessage.Published);
            Assert.IsNotNull(transportMessage.Message);
            Assert.AreEqual(typeof(TestMessage), transportMessage.MessageType);
            Assert.AreEqual(typeof(TestMessage).AssemblyQualifiedName, transportMessage.MessageTypeName);
            Assert.AreEqual("12345", transportMessage.ReceiptHandle);
            Assert.AreEqual("{ }", transportMessage.Body);
            Assert.AreEqual(3, transportMessage.AttemptNumber);
        }

        [TestMethod]
        public void MessageParserTests_FromSns_success()
        {
            string message = @"{
                ""Type"" : ""Notification"",
  ""MessageId"" : ""4f213ec0-a735-5359-bd02-6f5ecb8feeab"",
  ""TopicArn"" : ""arn:aws:sns:us-east-1:1234:JB_topic_arn"",
  ""Message"" : ""{\""Id\"":712738,\""Price\"":1.1}"",
  ""Timestamp"" : ""2017-05-23T16:59:38.893Z"",
  ""SignatureVersion"" : ""1"",
  ""Signature"" : """",
  ""SigningCertURL"" : ""https://google.com/"",
  ""UnsubscribeURL"" : ""https://google.com/"",
  ""MessageAttributes"" : {
                    ""BusVersion"" : { ""Type"":""String"",""Value"":""2.1.0.0""},
    ""SenderIpAddress"" : { ""Type"":""String"",""Value"":""127.0.0.1""},
    ""messageType"" : { ""Type"":""String"",""Value"":""" + typeof(TestMessage).AssemblyQualifiedName + @"""},
    ""SenderVersion"" : { ""Type"":""String"",""Value"":""1.2.0.0""},
    ""fromSns"" : { ""Type"":""String"",""Value"":""True""}
                }
            }";
            _messageAttributes.Clear();
            Message sourceMessage = new Message() { ReceiptHandle = "12345", Body = message, MessageAttributes = _messageAttributes };
            TransportMessage transportMessage = _parser.ParseMessage(sourceMessage);
            Assert.IsNotNull(transportMessage);
            Assert.IsTrue(transportMessage.MessageParsingSucceeded);
            Assert.IsTrue(transportMessage.Published);
            Assert.IsNotNull(transportMessage.Message);
            Assert.AreEqual(712738, ((TestMessage)transportMessage.Message).ID);
            Assert.AreEqual(1.1, ((TestMessage)transportMessage.Message).Price);
            Assert.AreEqual(typeof(TestMessage), transportMessage.MessageType);
            Assert.AreEqual(typeof(TestMessage).AssemblyQualifiedName, transportMessage.MessageTypeName);
            Assert.AreEqual("12345", transportMessage.ReceiptHandle);
            Assert.AreEqual(@"{""Id"":712738,""Price"":1.1}", transportMessage.Body);
        }

        [TestMethod]
        public void MessageParserTests_InvalidMessageFormat_success()
        {
            _messageAttributes.Clear();
            Message sourceMessage = new Message() { ReceiptHandle = "12345", Body = "{}", MessageAttributes = _messageAttributes };
            TransportMessage transportMessage = _parser.ParseMessage(sourceMessage);
            Assert.IsNotNull(transportMessage);
            Assert.IsFalse(transportMessage.MessageParsingSucceeded);
            Assert.IsFalse(transportMessage.Published);
            Assert.IsNull(transportMessage.Message);
            Assert.AreEqual(null, transportMessage.MessageType);
            Assert.AreEqual(null, transportMessage.MessageTypeName);
            Assert.AreEqual("12345", transportMessage.ReceiptHandle);
            Assert.AreEqual("{}", transportMessage.Body);
        }

        [TestMethod]
        public void MessageParserTests_FromSnsEnvelope_success()
        {
            _messageAttributes["fromSns"] = new MessageAttributeValue();
            Message sourceMessage = new Message() { ReceiptHandle = "12345", Body = "{ }", MessageAttributes = _messageAttributes };
            TransportMessage transportMessage = _parser.ParseMessage(sourceMessage);
            Assert.IsNotNull(transportMessage);
            Assert.IsTrue(transportMessage.MessageParsingSucceeded);
            Assert.IsTrue(transportMessage.Published);
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
            Message sourceMessage = new Message() { ReceiptHandle = "12345", Body = "{}", MessageAttributes = _messageAttributes, Attributes = _attributes };
            TransportMessage transportMessage = _parser.ParseMessage(sourceMessage);
            Assert.IsNotNull(transportMessage);
            Assert.IsFalse(transportMessage.MessageParsingSucceeded);
            Assert.IsNull(transportMessage.Message);
            Assert.IsNull(transportMessage.MessageType);
            Assert.AreEqual("Invalid Type", transportMessage.MessageTypeName);
            Assert.AreEqual("12345", transportMessage.ReceiptHandle);
            Assert.AreEqual("{}", transportMessage.Body);
            Assert.AreEqual(3, transportMessage.AttemptNumber);
        }

        [TestMethod]
        public void MessageParserTests_Deserialization_failure_message_body()
        {
            Message sourceMessage = new Message() { ReceiptHandle = "12345", Body = "sdsdsd", MessageAttributes = _messageAttributes, Attributes = _attributes };
            TransportMessage transportMessage = _parser.ParseMessage(sourceMessage);
            Assert.IsNotNull(transportMessage);
            Assert.IsFalse(transportMessage.MessageParsingSucceeded);
            Assert.IsNull(transportMessage.Message);
            Assert.AreEqual(typeof(TestMessage), transportMessage.MessageType);
            Assert.AreEqual(typeof(TestMessage).AssemblyQualifiedName, transportMessage.MessageTypeName);
            Assert.AreEqual("12345", transportMessage.ReceiptHandle);
            Assert.AreEqual("sdsdsd", transportMessage.Body);
            Assert.AreEqual(3, transportMessage.AttemptNumber);
        }
    }
}
