using System;
using System.Collections.Generic;
using Common.Logging;
using JungleBus.Interfaces;
using JungleBus.Interfaces.IoC;
using JungleBus.IoC;
using JungleBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JungleBus.Tests.Messaging
{
    [TestClass]
    public class MessageProcessorTests
    {
        private MessageProcessor _processor;
        private Mock<IObjectBuilder> _objectBuilder;
        private Dictionary<Type, HashSet<Type>> _typeMapping;
        private static int _testHandler1Called;
        private static int _testHandler2Called;
        private static int _testHandler3Called;

        [TestInitialize]
        public void TestInitialize()
        {
            _testHandler1Called = 0;
            _testHandler2Called = 0;
            _testHandler3Called = 0;
            _typeMapping = new Dictionary<Type, HashSet<Type>>()
            {
                { typeof(TestMessage), new HashSet<Type>() { typeof(TestHandler), typeof(TestHandler2) } },
                { typeof(TestHandler), new HashSet<Type>() { typeof(TestHandler), typeof(TestHandler2) } },
            };
            _objectBuilder = new Mock<IObjectBuilder>(MockBehavior.Strict);
            _objectBuilder.Setup(x => x.GetNestedBuilder()).Returns(_objectBuilder.Object);
            _objectBuilder.Setup(x => x.Dispose());
            _objectBuilder.Setup(x => x.RegisterInstance<ILog>(It.IsAny<ILog>()));
            _objectBuilder.Setup(x => x.GetValue(It.IsAny<Type>())).Returns<Type>(x => Activator.CreateInstance(x));

            _processor = new MessageProcessor(_objectBuilder.Object, _typeMapping);
        }

        [TestMethod]
        public void MessageProcessorTests_process()
        {
            bool result = _processor.ProcessMessage(new TransportMessage() { MessageType = typeof(TestMessage), Message = new TestMessage() }, null);
            Assert.IsTrue(result);
            Assert.AreEqual(1, _testHandler1Called);
            Assert.AreEqual(1, _testHandler2Called);
            Assert.AreEqual(0, _testHandler3Called);
        }

        [TestMethod]
        public void MessageProcessorTests_Exception_Thrown_in_Handler()
        {
            _typeMapping[typeof(TestMessage)].Add(typeof(TestHandler3));
            bool result = _processor.ProcessMessage(new TransportMessage() { MessageType = typeof(TestMessage), Message = new TestMessage() }, null);
            Assert.IsFalse(result);
            Assert.AreEqual(1, _testHandler1Called);
            Assert.AreEqual(1, _testHandler2Called);
            Assert.AreEqual(1, _testHandler3Called);
        }

        [TestMethod]
        public void MessageProcessorTests_Unknown_message()
        {
            _typeMapping.Remove(typeof(TestMessage));
            bool result = _processor.ProcessMessage(new TransportMessage() { MessageType = typeof(TestMessage), Message = new TestMessage() }, null);
            Assert.IsFalse(result);
            Assert.AreEqual(0, _testHandler1Called);
            Assert.AreEqual(0, _testHandler2Called);
            Assert.AreEqual(0, _testHandler3Called);
        }

        class TestHandler : IHandleMessage<TestMessage>
        {
            public void Handle(TestMessage message)
            {
                ++_testHandler1Called;
            }
        }

        class TestHandler2 : IHandleMessage<TestMessage>
        {
            public void Handle(TestMessage message)
            {
                ++_testHandler2Called;
            }
        }

        class TestHandler3 : IHandleMessage<TestMessage>
        {
            public void Handle(TestMessage message)
            {
                ++_testHandler3Called;
                throw new Exception("Exception!");
            }
        }
    }
}
