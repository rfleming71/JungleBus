using System;
using System.Collections.Generic;
using Common.Logging;
using JungleBus.Interfaces;
using JungleBus.Interfaces.IoC;
using JungleBus.Messaging;
using JungleBus.Queue.Messaging;
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
        private Dictionary<Type, HashSet<Type>> _faultHandlers;
        private static int _testHandler1Called;
        private static int _testHandler2Called;
        private static int _testHandler3Called;
        private static int _testFaultHandler1Called;
        private static int _testFaultHandler2Called;
        private static int _testFaultHandler3Called;
        private static int _transportMessageFaultHandler;
        private TransportMessage _message;

        [TestInitialize]
        public void TestInitialize()
        {
            _testHandler1Called = 0;
            _testHandler2Called = 0;
            _testHandler3Called = 0;
            _testFaultHandler1Called = 0;
            _testFaultHandler2Called = 0;
            _testFaultHandler3Called = 0;
            _transportMessageFaultHandler = 0;
            _typeMapping = new Dictionary<Type, HashSet<Type>>()
            {
                { typeof(TestMessage), new HashSet<Type>() { typeof(TestHandler), typeof(TestHandler2) } },
                { typeof(TestHandler), new HashSet<Type>() { typeof(TestHandler), typeof(TestHandler2) } },
            };

            _faultHandlers = new Dictionary<Type, HashSet<Type>>()
            {
                { typeof(TestMessage), new HashSet<Type>() { typeof(TestFaultHandler1), typeof(TestFaultHandler2) } },
                { typeof(TestHandler), new HashSet<Type>() { typeof(TestFaultHandler1), typeof(TestFaultHandler2) } },
            };

            _objectBuilder = new Mock<IObjectBuilder>(MockBehavior.Strict);
            _objectBuilder.Setup(x => x.GetNestedBuilder()).Returns(_objectBuilder.Object);
            _objectBuilder.Setup(x => x.Dispose());
            _objectBuilder.Setup(x => x.RegisterInstance<ILog>(It.IsAny<ILog>()));
            _objectBuilder.Setup(x => x.GetValue(It.IsAny<Type>())).Returns<Type>(x => Activator.CreateInstance(x));

            _processor = new MessageProcessor(_typeMapping, _faultHandlers, _objectBuilder.Object);
            _message = new TransportMessage() { MessageType = typeof(TestMessage), Message = new TestMessage(), MessageParsingSucceeded = true, };
        }

        [TestMethod]
        public void MessageProcessorTests_process()
        {
            MessageProcessingResult result = _processor.ProcessMessage(_message);
            Assert.IsTrue(result.WasSuccessful);
            Assert.AreEqual(1, _testHandler1Called);
            Assert.AreEqual(1, _testHandler2Called);
            Assert.AreEqual(0, _testHandler3Called);
        }

        [TestMethod]
        public void MessageProcessorTests_Exception_Thrown_in_Handler()
        {
            _typeMapping[typeof(TestMessage)].Add(typeof(TestHandler3));
            MessageProcessingResult result = _processor.ProcessMessage(_message);
            Assert.IsFalse(result.WasSuccessful);
            Assert.AreEqual(1, _testHandler1Called);
            Assert.AreEqual(1, _testHandler2Called);
            Assert.AreEqual(1, _testHandler3Called);
        }

        [TestMethod]
        public void MessageProcessorTests_Unknown_message()
        {
            _typeMapping.Remove(typeof(TestMessage));
            MessageProcessingResult result = _processor.ProcessMessage(_message);
            Assert.IsFalse(result.WasSuccessful);
            Assert.AreEqual(0, _testHandler1Called);
            Assert.AreEqual(0, _testHandler2Called);
            Assert.AreEqual(0, _testHandler3Called);
        }

        [TestMethod]
        public void MessageProcessorTests_process_fault()
        {
            _processor.ProcessFaultedMessage(_message, new Exception("Foo!"));
            Assert.AreEqual(1, _testFaultHandler1Called);
            Assert.AreEqual(1, _testFaultHandler2Called);
            Assert.AreEqual(0, _testFaultHandler3Called);
        }

        [TestMethod]
        public void MessageProcessorTests_process_general_and_message_handlers()
        {
            _faultHandlers[typeof(TransportMessage)] = new HashSet<Type>() { typeof(TestFaultHandler) };
            _processor.ProcessFaultedMessage(_message, new Exception("Foo!"));
            Assert.AreEqual(1, _testFaultHandler1Called);
            Assert.AreEqual(1, _testFaultHandler2Called);
            Assert.AreEqual(0, _testFaultHandler3Called);
            Assert.AreEqual(1, _transportMessageFaultHandler);
        }

        [TestMethod]
        public void MessageProcessorTests_process_general_handlers()
        {
            _faultHandlers.Remove(typeof(TestMessage));
            _faultHandlers[typeof(TransportMessage)] = new HashSet<Type>() { typeof(TestFaultHandler) };
            _processor.ProcessFaultedMessage(_message, new Exception("Foo!"));
            Assert.AreEqual(0, _testFaultHandler1Called);
            Assert.AreEqual(0, _testFaultHandler2Called);
            Assert.AreEqual(0, _testFaultHandler3Called);
            Assert.AreEqual(1, _transportMessageFaultHandler);
        }

        [TestMethod]
        public void MessageProcessorTests_process_general_handlers_on_parse_failure()
        {
            _faultHandlers[typeof(TransportMessage)] = new HashSet<Type>() { typeof(TestFaultHandler) };
            _message.MessageParsingSucceeded = false;
            _processor.ProcessFaultedMessage(_message, new Exception("Foo!"));
            Assert.AreEqual(0, _testFaultHandler1Called);
            Assert.AreEqual(0, _testFaultHandler2Called);
            Assert.AreEqual(0, _testFaultHandler3Called);
            Assert.AreEqual(1, _transportMessageFaultHandler);
        }

        [TestMethod]
        public void MessageProcessorTests_Exception_Thrown_Fault_Handler()
        {
            _faultHandlers[typeof(TestMessage)].Add(typeof(TestFaultHandler3));
            _processor.ProcessFaultedMessage(_message, new Exception("Foo!"));
            Assert.AreEqual(1, _testFaultHandler1Called);
            Assert.AreEqual(1, _testFaultHandler2Called);
            Assert.AreEqual(1, _testFaultHandler3Called);
        }

        #region TEST HANDLERS

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

        class TestFaultHandler1 : IHandleMessageFaults<TestMessage>
        {
            public void Handle(TestMessage message, Exception ex)
            {
                ++_testFaultHandler1Called;
                throw new Exception("Exception!");
            }
        }

        class TestFaultHandler2 : IHandleMessageFaults<TestMessage>
        {
            public void Handle(TestMessage message, Exception ex)
            {
                ++_testFaultHandler2Called;
            }
        }

        class TestFaultHandler3 : IHandleMessageFaults<TestMessage>
        {
            public void Handle(TestMessage message, Exception ex)
            {
                ++_testFaultHandler3Called;
                throw new Exception("Exception!");
            }
        }

        class TestFaultHandler : IHandleMessageFaults<TransportMessage>
        {
            public void Handle(TransportMessage message, Exception ex)
            {
                ++_transportMessageFaultHandler;
            }
        }

        #endregion TEST HANDLERS
    }
}
