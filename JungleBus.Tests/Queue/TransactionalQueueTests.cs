using System;
using System.Collections.Generic;
using System.Transactions;
using JungleBus.Aws.Sqs;
using JungleBus.Interfaces;
using JungleBus.Interfaces.Serialization;
using JungleBus.Messaging;
using JungleBus.Queue;
using JungleBus.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JungleBus.Tests
{
    [TestClass]
    public class TransactionalQueueTests
    {
        private TransactionalQueue _queueUnderTest;
        private Mock<ISqsQueue> _sqsQueue;
        private Mock<IMessageLogger> _messageLogger;

        [TestInitialize]
        public void TestInitialize()
        {
            _sqsQueue = new Mock<ISqsQueue>(MockBehavior.Strict);
            _sqsQueue.Setup(x => x.AddMessage(It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>>()));

            _messageLogger = new Mock<IMessageLogger>(MockBehavior.Strict);
            _messageLogger.Setup(x => x.OutboundLogMessage(It.IsAny<string>(), It.IsAny<string>()));

            _queueUnderTest = new TransactionalQueue(_sqsQueue.Object, _messageLogger.Object);
        }

        [TestMethod]
        public void TransactionalQueueTests_NoTransaction_PublishLocal_Message()
        {
            _queueUnderTest.Send(new TestMessage());
            _sqsQueue.Verify(x => x.AddMessage(It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>>()), Times.Once());
            _messageLogger.Verify(x => x.OutboundLogMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public void TransactionalQueueTests_NoTransaction_PublishLocal_Builds_Message()
        {
            _queueUnderTest.Send<TestMessage>(x => x.Name = "Name");
            _sqsQueue.Verify(x => x.AddMessage(It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>>()), Times.Once());
            _messageLogger.Verify(x => x.OutboundLogMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }
        [TestMethod]
        public void TransactionalQueueTests_Transaction_PublishLocal_Message_Build_OnCommit()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                _queueUnderTest.Send<TestMessage>(x => x.Name = "Name1");
                _queueUnderTest.Send<TestMessage>(x => x.Name = "Name2");
                scope.Complete();
            }

            _sqsQueue.Verify(x => x.AddMessage(It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>>()), Times.Exactly(2));
            _messageLogger.Verify(x => x.OutboundLogMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [TestMethod]
        public void TransactionalQueueTests_Transaction_PublishLocal_Message_Build_OnRollback()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                _queueUnderTest.Send<TestMessage>(x => x.Name = "Name1");
                _queueUnderTest.Send<TestMessage>(x => x.Name = "Name2");
                scope.Dispose();
            }
            _sqsQueue.Verify(x => x.AddMessage(It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>>()), Times.Never());
            _messageLogger.Verify(x => x.OutboundLogMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void TransactionalQueueTests_Transaction_PublishLocal_Message_Build_OnRollback_Then_Commit()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                _queueUnderTest.Send<TestMessage>(x => x.Name = "Name1");
                _queueUnderTest.Send<TestMessage>(x => x.Name = "Name2");
                scope.Dispose();
            }

            using (TransactionScope scope = new TransactionScope())
            {
                _queueUnderTest.Send<TestMessage>(x => x.Name = "Name11");
                _queueUnderTest.Send<TestMessage>(x => x.Name = "Name21");
                scope.Complete();
            }

            _sqsQueue.Verify(x => x.AddMessage(It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>>()), Times.Exactly(2));
            _messageLogger.Verify(x => x.OutboundLogMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [TestMethod]
        public void TransactionalQueueTests_Transaction_PublishLocal_Message_OnCommit()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                _queueUnderTest.Send(new TestMessage() { Name = "Name1" });
                _queueUnderTest.Send(new TestMessage() { Name = "Name2" });
                scope.Complete();
            }

            _sqsQueue.Verify(x => x.AddMessage(It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>>()), Times.Exactly(2));
            _messageLogger.Verify(x => x.OutboundLogMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [TestMethod]
        public void TransactionalQueueTests_Transaction_PublishLocal_Message_OnRollback()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                _queueUnderTest.Send(new TestMessage() { Name = "Name1" });
                _queueUnderTest.Send(new TestMessage() { Name = "Name2" });
                scope.Dispose();
            }

            _sqsQueue.Verify(x => x.AddMessage(It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>>()), Times.Never());
            _messageLogger.Verify(x => x.OutboundLogMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void TransactionalQueueTests_Transaction_PublishLocal_Message_OnRollback_Then_Commit()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                _queueUnderTest.Send(new TestMessage() { Name = "Name1" });
                _queueUnderTest.Send(new TestMessage() { Name = "Name2" });
                scope.Dispose();
            }

            using (TransactionScope scope = new TransactionScope())
            {
                _queueUnderTest.Send(new TestMessage() { Name = "Name11" });
                _queueUnderTest.Send(new TestMessage() { Name = "Name21" });
                scope.Complete();
            }

            _sqsQueue.Verify(x => x.AddMessage(It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>>()), Times.Exactly(2));
            _messageLogger.Verify(x => x.OutboundLogMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }
    }
}
