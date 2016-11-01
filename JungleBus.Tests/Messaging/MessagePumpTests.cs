using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JungleBus.Interfaces;
using JungleBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JungleBus.Tests.Messaging
{
    [TestClass]
    public class MessagePumpTests
    {
        private MessagePump _messagePump;
        private Mock<IMessageQueue> _queue;
        private Mock<IMessageProcessor> _messageProcessor;
        private Mock<IBus> _bus;
        private Mock<IMessageLogger> _messageLogger;
        private TransportMessage _message;
        private const int MaxTryCount = 5;

        [TestInitialize]
        public void TestInitialized()
        {
            _messageLogger = new Mock<IMessageLogger>();
            _message = new TransportMessage()
            {
                ReceiptHandle = "123",
                MessageParsingSucceeded = true,
                Message = "new message",
            };
            _bus = new Mock<IBus>(MockBehavior.Strict);
            _queue = new Mock<IMessageQueue>(MockBehavior.Strict);
            _messageProcessor = new Mock<IMessageProcessor>(MockBehavior.Strict);
            _messageProcessor.Setup(x => x.ProcessMessage(It.IsAny<TransportMessage>(), It.IsAny<IBus>())).Returns(new MessageProcessingResult() { WasSuccessful = true });

            _messagePump = new MessagePump(_queue.Object, MaxTryCount, _messageProcessor.Object, _messageLogger.Object, _bus.Object, 1);

            _queue.Setup(x => x.GetMessages(It.IsAny<CancellationToken>())).Returns(Task.FromResult((IEnumerable<TransportMessage>)new[] { _message }))
                .Callback(() => _messagePump.Stop());
        }

        [TestMethod]
        public void MessagePumpTests_SinglePass_success()
        {
            _messagePump.Run();
            _queue.Verify(x => x.GetMessages(It.IsAny<CancellationToken>()), Times.Once());
            _queue.Verify(x => x.RemoveMessage(It.Is<TransportMessage>(t => t.ReceiptHandle == "123")), Times.Once());
            _messageProcessor.Verify(x => x.ProcessMessage(It.IsAny<TransportMessage>(), It.IsAny<IBus>()), Times.Once());
        }

        [TestMethod]
        public void MessagePumpTests_SinglePass_message_parse_failure()
        {
            _message.MessageParsingSucceeded = false;
            _messagePump.Run();
            _queue.Verify(x => x.GetMessages(It.IsAny<CancellationToken>()), Times.Once());
            _queue.Verify(x => x.RemoveMessage(It.IsAny<TransportMessage>()), Times.Never());
            _messageProcessor.Verify(x => x.ProcessMessage(It.IsAny<TransportMessage>(), It.IsAny<IBus>()), Times.Never());
        }

        [TestMethod]
        public void MessagePumpTests_SinglePass_processing_failure()
        {
            _message.AttemptNumber = 1;
            _messageProcessor.Setup(x => x.ProcessMessage(It.IsAny<TransportMessage>(), It.IsAny<IBus>())).Returns(new MessageProcessingResult() { WasSuccessful = false });
            _messagePump.Run();
            _queue.Verify(x => x.GetMessages(It.IsAny<CancellationToken>()), Times.Once());
            _queue.Verify(x => x.RemoveMessage(It.IsAny<TransportMessage>()), Times.Never());
            _messageProcessor.Verify(x => x.ProcessMessage(It.IsAny<TransportMessage>(), It.IsAny<IBus>()), Times.Once());
            _messageProcessor.Verify(x => x.ProcessFaultedMessage(It.IsAny<TransportMessage>(), It.IsAny<IBus>(), It.IsAny<Exception>()), Times.Never());
        }

        [TestMethod]
        public void MessagePumpTests_SinglePass_FinalPass_processing_failure()
        {
            _message.AttemptNumber = MaxTryCount;
            _messageProcessor.Setup(x => x.ProcessMessage(It.IsAny<TransportMessage>(), It.IsAny<IBus>())).Returns(new MessageProcessingResult() { WasSuccessful = false, Exception = new Exception()});
            _messagePump.Run();
            _queue.Verify(x => x.GetMessages(It.IsAny<CancellationToken>()), Times.Once());
            _queue.Verify(x => x.RemoveMessage(It.IsAny<TransportMessage>()), Times.Never());
            _messageProcessor.Verify(x => x.ProcessMessage(It.IsAny<TransportMessage>(), It.IsAny<IBus>()), Times.Once());
            _messageProcessor.Verify(x => x.ProcessFaultedMessage(It.IsAny<TransportMessage>(), It.IsAny<IBus>(), It.IsAny<Exception>()), Times.Once());
        }
    }
}
