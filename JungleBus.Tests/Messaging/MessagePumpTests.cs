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
        private TransportMessage _message;

        [TestInitialize]
        public void TestInitialized()
        {
            _message = new TransportMessage()
            {
                ReceiptHandle = "123",
                MessageParsingSucceeded = true,
                Message = "new message",
            };
            _bus = new Mock<IBus>(MockBehavior.Strict);
            _queue = new Mock<IMessageQueue>(MockBehavior.Strict);
            _messageProcessor = new Mock<IMessageProcessor>(MockBehavior.Strict);
            _messageProcessor.Setup(x => x.ProcessMessage(It.IsAny<TransportMessage>(), It.IsAny<IBus>())).Returns(true);

            _messagePump = new MessagePump(_queue.Object, _messageProcessor.Object, _bus.Object, 1);

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
            _messageProcessor.Setup(x => x.ProcessMessage(It.IsAny<TransportMessage>(), It.IsAny<IBus>())).Returns(false);
            _messagePump.Run();
            _queue.Verify(x => x.GetMessages(It.IsAny<CancellationToken>()), Times.Once());
            _queue.Verify(x => x.RemoveMessage(It.IsAny<TransportMessage>()), Times.Never());
            _messageProcessor.Verify(x => x.ProcessMessage(It.IsAny<TransportMessage>(), It.IsAny<IBus>()), Times.Once());
        }
    }
}
