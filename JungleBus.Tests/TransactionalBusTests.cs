using System;
using System.Transactions;
using JungleBus.Interfaces;
using JungleBus.Interfaces.Serialization;
using JungleBus.Messaging;
using JungleBus.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JungleBus.Tests
{
    [TestClass]
    public class TransactionalBusTests
    {
        private TransactionalBus _busUnderTest;
        private Mock<IMessagePublisher> _messagePublisher;
        private Mock<IQueue> _messageQueue;

        [TestInitialize]
        public void TestInitialize()
        {
            _messagePublisher = new Mock<IMessagePublisher>(MockBehavior.Strict);
            _messagePublisher.Setup(x => x.Publish(It.IsAny<string>(), It.IsAny<Type>()));

            _messageQueue = new Mock<IQueue>(MockBehavior.Strict);
            _messageQueue.Setup(x => x.Send(It.IsAny<TestMessage>()));
            _messageQueue.Setup(x => x.Send(It.IsAny<Action<TestMessage>>()));

            _busUnderTest = new TransactionalBus(_messagePublisher.Object, _messageQueue.Object);
        }

        #region PUBLISH MESSAGE TESTS

        [TestMethod]
        public void TransactionalBusTests_NoTransaction_Publishes_Message()
        {
            _busUnderTest.Publish(new TestMessage());
            _messagePublisher.Verify(x => x.Publish("{\"ID\":0,\"Price\":0.0,\"Name\":null,\"Modified\":\"0001-01-01T00:00:00\"}", typeof(TestMessage)), Times.Once());
        }

        [TestMethod]
        public void TransactionalBusTests_NoTransaction_Publishes_Builds_Message()
        {
            _busUnderTest.Publish<TestMessage>(x => x.Name = "Name");
            _messagePublisher.Verify(x => x.Publish("{\"ID\":0,\"Price\":0.0,\"Name\":\"Name\",\"Modified\":\"0001-01-01T00:00:00\"}", typeof(TestMessage)), Times.Once());
        }

        [TestMethod]
        public void TransactionalBusTests_Transaction_Publishes_Message_Build_OnCommit()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                _busUnderTest.Publish<TestMessage>(x => x.Name = "Name1");
                _busUnderTest.Publish<TestMessage>(x => x.Name = "Name2");
                scope.Complete();
            }
            _messagePublisher.Verify(x => x.Publish("{\"ID\":0,\"Price\":0.0,\"Name\":\"Name1\",\"Modified\":\"0001-01-01T00:00:00\"}", typeof(TestMessage)), Times.Once());
            _messagePublisher.Verify(x => x.Publish("{\"ID\":0,\"Price\":0.0,\"Name\":\"Name2\",\"Modified\":\"0001-01-01T00:00:00\"}", typeof(TestMessage)), Times.Once());
        }

        [TestMethod]
        public void TransactionalBusTests_Transaction_Publishes_Message_Build_OnRollback()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                _busUnderTest.Publish<TestMessage>(x => x.Name = "Name1");
                _busUnderTest.Publish<TestMessage>(x => x.Name = "Name2");
                scope.Dispose();
            }
            _messagePublisher.Verify(x => x.Publish(It.IsAny<string>(), typeof(TestMessage)), Times.Never());
        }

        [TestMethod]
        public void TransactionalBusTests_Transaction_Publishes_Message_Build_OnRollback_Then_Commit()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                _busUnderTest.Publish<TestMessage>(x => x.Name = "Name1");
                _busUnderTest.Publish<TestMessage>(x => x.Name = "Name2");
                scope.Dispose();
            }

            using (TransactionScope scope = new TransactionScope())
            {
                _busUnderTest.Publish<TestMessage>(x => x.Name = "Name11");
                _busUnderTest.Publish<TestMessage>(x => x.Name = "Name21");
                scope.Complete();
            }
            _messagePublisher.Verify(x => x.Publish("{\"ID\":0,\"Price\":0.0,\"Name\":\"Name11\",\"Modified\":\"0001-01-01T00:00:00\"}", typeof(TestMessage)), Times.Once());
            _messagePublisher.Verify(x => x.Publish("{\"ID\":0,\"Price\":0.0,\"Name\":\"Name21\",\"Modified\":\"0001-01-01T00:00:00\"}", typeof(TestMessage)), Times.Once());
        }

        [TestMethod]
        public void TransactionalBusTests_Transaction_Publishes_Message_OnCommit()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                _busUnderTest.Publish(new TestMessage() { Name = "Name1" });
                _busUnderTest.Publish(new TestMessage() { Name = "Name2" });
                scope.Complete();
            }
            _messagePublisher.Verify(x => x.Publish("{\"ID\":0,\"Price\":0.0,\"Name\":\"Name1\",\"Modified\":\"0001-01-01T00:00:00\"}", typeof(TestMessage)), Times.Once());
            _messagePublisher.Verify(x => x.Publish("{\"ID\":0,\"Price\":0.0,\"Name\":\"Name2\",\"Modified\":\"0001-01-01T00:00:00\"}", typeof(TestMessage)), Times.Once());
        }

        [TestMethod]
        public void TransactionalBusTests_Transaction_Publishes_Message_OnRollback()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                _busUnderTest.Publish(new TestMessage() { Name = "Name1" });
                _busUnderTest.Publish(new TestMessage() { Name = "Name2" });
                scope.Dispose();
            }
            _messagePublisher.Verify(x => x.Publish(It.IsAny<string>(), typeof(TestMessage)), Times.Never());
        }

        [TestMethod]
        public void TransactionalBusTests_Transaction_Publishes_Message_OnRollback_Then_Commit()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                _busUnderTest.Publish(new TestMessage() { Name = "Name1" });
                _busUnderTest.Publish(new TestMessage() { Name = "Name2" });
                scope.Dispose();
            }

            using (TransactionScope scope = new TransactionScope())
            {
                _busUnderTest.Publish(new TestMessage() { Name = "Name11" });
                _busUnderTest.Publish(new TestMessage() { Name = "Name21" });
                scope.Complete();
            }
            _messagePublisher.Verify(x => x.Publish("{\"ID\":0,\"Price\":0.0,\"Name\":\"Name11\",\"Modified\":\"0001-01-01T00:00:00\"}", typeof(TestMessage)), Times.Once());
            _messagePublisher.Verify(x => x.Publish("{\"ID\":0,\"Price\":0.0,\"Name\":\"Name21\",\"Modified\":\"0001-01-01T00:00:00\"}", typeof(TestMessage)), Times.Once());
        }

        #endregion PUBLISH MESSAGE TESTS

        #region PUBLISH LOCAL TESTS

        [TestMethod]
        public void TransactionalBusTests_NoTransaction_PublishLocal_Message()
        {
            _busUnderTest.PublishLocal(new TestMessage());
            _messageQueue.Verify(x => x.Send(It.IsAny<TestMessage>()), Times.Once());
            _messagePublisher.Verify(x => x.Publish(It.IsAny<string>(), It.IsAny<Type>()), Times.Never());
        }

        [TestMethod]
        public void TransactionalBusTests_NoTransaction_PublishLocal_Builds_Message()
        {
            _busUnderTest.PublishLocal<TestMessage>(x => x.Name = "Name");
            _messageQueue.Verify(x => x.Send(It.IsAny<Action<TestMessage>>()), Times.Once());
            _messagePublisher.Verify(x => x.Publish(It.IsAny<string>(), It.IsAny<Type>()), Times.Never());
        }
        [TestMethod]
        public void TransactionalBusTests_Transaction_PublishLocal_Message_Build_OnCommit()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                _busUnderTest.PublishLocal<TestMessage>(x => x.Name = "Name1");
                _busUnderTest.PublishLocal<TestMessage>(x => x.Name = "Name2");
                scope.Complete();
            }

            _messagePublisher.Verify(x => x.Publish(It.IsAny<string>(), typeof(TestMessage)), Times.Never());
            _messageQueue.Verify(x => x.Send(It.IsAny<Action<TestMessage>>()), Times.Exactly(2));
        }

        [TestMethod]
        public void TransactionalBusTests_Transaction_PublishLocal_Message_Build_OnRollback()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                _busUnderTest.PublishLocal<TestMessage>(x => x.Name = "Name1");
                _busUnderTest.PublishLocal<TestMessage>(x => x.Name = "Name2");
                scope.Dispose();
            }
            _messagePublisher.Verify(x => x.Publish(It.IsAny<string>(), It.IsAny<Type>()), Times.Never());
            _messageQueue.Verify(x => x.Send(It.IsAny<Action<TestMessage>>()), Times.Exactly(2));
        }

        [TestMethod]
        public void TransactionalBusTests_Transaction_PublishLocal_Message_Build_OnRollback_Then_Commit()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                _busUnderTest.PublishLocal<TestMessage>(x => x.Name = "Name1");
                _busUnderTest.PublishLocal<TestMessage>(x => x.Name = "Name2");
                scope.Dispose();
            }

            using (TransactionScope scope = new TransactionScope())
            {
                _busUnderTest.PublishLocal<TestMessage>(x => x.Name = "Name11");
                _busUnderTest.PublishLocal<TestMessage>(x => x.Name = "Name21");
                scope.Complete();
            }

            _messagePublisher.Verify(x => x.Publish(It.IsAny<string>(), typeof(TestMessage)), Times.Never());
            _messageQueue.Verify(x => x.Send(It.IsAny<Action<TestMessage>>()), Times.Exactly(4));
        }

        [TestMethod]
        public void TransactionalBusTests_Transaction_PublishLocal_Message_OnCommit()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                _busUnderTest.PublishLocal(new TestMessage() { Name = "Name1" });
                _busUnderTest.PublishLocal(new TestMessage() { Name = "Name2" });
                scope.Complete();
            }

            _messagePublisher.Verify(x => x.Publish(It.IsAny<string>(), typeof(TestMessage)), Times.Never());
            _messageQueue.Verify(x => x.Send(It.IsAny<TestMessage>()), Times.Exactly(2));
        }

        [TestMethod]
        public void TransactionalBusTests_Transaction_PublishLocal_Message_OnRollback()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                _busUnderTest.PublishLocal(new TestMessage() { Name = "Name1" });
                _busUnderTest.PublishLocal(new TestMessage() { Name = "Name2" });
                scope.Dispose();
            }

            _messagePublisher.Verify(x => x.Publish(It.IsAny<string>(), It.IsAny<Type>()), Times.Never());
            _messageQueue.Verify(x => x.Send(It.IsAny<Action<TestMessage>>()), Times.Never());
        }

        [TestMethod]
        public void TransactionalBusTests_Transaction_PublishLocal_Message_OnRollback_Then_Commit()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                _busUnderTest.PublishLocal(new TestMessage() { Name = "Name1" });
                _busUnderTest.PublishLocal(new TestMessage() { Name = "Name2" });
                scope.Dispose();
            }

            using (TransactionScope scope = new TransactionScope())
            {
                _busUnderTest.PublishLocal(new TestMessage() { Name = "Name11" });
                _busUnderTest.PublishLocal(new TestMessage() { Name = "Name21" });
                scope.Complete();
            }

            _messagePublisher.Verify(x => x.Publish(It.IsAny<string>(), typeof(TestMessage)), Times.Never());
            _messageQueue.Verify(x => x.Send(It.IsAny<TestMessage>()), Times.Exactly(4));
        }

        #endregion PUBLISH LOCAL TESTS
    }
}
