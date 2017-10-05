using Amazon;
using JungleBus.Configuration;
using JungleBus.Interfaces.Exceptions;
using JungleQueue.Interfaces.IoC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JungleBus.Tests.Configuration
{
    [TestClass]
    public class ReceiveConfigurationExtensionsTests
    {
        [TestMethod]
        public void ReceiveConfigurationExtensionsTests_SetInputQueue_SetupsQueueConfig()
        {
            BusConfiguration config = new BusConfiguration()
            {
                ObjectBuilder = new Mock<IObjectBuilder>().Object,
            };
            config = config.SetInputQueue("SqsName", RegionEndpoint.USEast1) as BusConfiguration;
            Assert.IsNotNull(config.InputQueueConfiguration);
            Assert.IsNotNull(config.ObjectBuilder);
            Assert.AreEqual("SqsName", config.InputQueueConfiguration.QueueName);
        }

        [TestMethod]
        public void ReceiveConfigurationExtensionsTests_SetInputQueue_PrependsBusName()
        {
            BusConfiguration config = new BusConfiguration()
            {
                ObjectBuilder = new Mock<IObjectBuilder>().Object,
                BusName = "Test",
            };
            config = config.SetInputQueue("SqsName", RegionEndpoint.USEast1) as BusConfiguration;
            Assert.AreEqual("Test_SqsName", config.InputQueueConfiguration.QueueName);
        }

        [TestMethod]
        [ExpectedException(typeof(JungleBusConfigurationException))]
        public void ReceiveConfigurationExtensionsTests_SetInputQueue_NullConfiguration()
        {
            (null as IBusConfiguration).SetInputQueue("SqsName", RegionEndpoint.USEast1);
        }

        [TestMethod]
        [ExpectedException(typeof(JungleBusConfigurationException))]
        public void ReceiveConfigurationExtensionsTests_SetInputQueue_NullSqsName()
        {
            BusConfiguration config = new BusConfiguration();
            config.SetInputQueue(null, RegionEndpoint.USEast1);
        }

        [TestMethod]
        [ExpectedException(typeof(JungleBusConfigurationException))]
        public void ReceiveConfigurationExtensionsTests_SetInputQueue_WhitespaceSqsName()
        {
            BusConfiguration config = new BusConfiguration();
            config.SetInputQueue("   ", RegionEndpoint.USEast1);
        }

        [TestMethod]
        [ExpectedException(typeof(JungleBusConfigurationException))]
        public void ReceiveConfigurationExtensionsTests_SetInputQueue_NullRegion()
        {
            BusConfiguration config = new BusConfiguration();
            config.SetInputQueue("Queue", null);
        }

        [TestMethod]
        [ExpectedException(typeof(JungleBusConfigurationException))]
        public void ReceiveConfigurationExtensionsTests_SetSqsPollWaitTime_NullConfiguration()
        {
            (null as IConfigureEventReceiving).SetSqsPollWaitTime(10);
        }

        [TestMethod]
        [ExpectedException(typeof(JungleBusConfigurationException))]
        public void ReceiveConfigurationExtensionsTests_SetNumberOfPollingInstances_NullConfiguration()
        {
            (null as IConfigureEventReceiving).SetNumberOfPollingInstances(10);
        }

        [TestMethod]
        [ExpectedException(typeof(JungleBusConfigurationException))]
        public void ReceiveConfigurationExtensionsTests_UsingEventHandlersFromEntryAssembly_NullConfiguration()
        {
            (null as IConfigureEventReceiving).UsingEventHandlersFromEntryAssembly();
        }

        [TestMethod]
        [ExpectedException(typeof(JungleBusConfigurationException))]
        public void ReceiveConfigurationExtensionsTests_UsingEventHandlersFromEntryAssembly_NullInputConfiguration()
        {
            BusConfiguration config = new BusConfiguration();
            (config as IConfigureEventReceiving).UsingEventHandlersFromEntryAssembly();
        }
    }
}
