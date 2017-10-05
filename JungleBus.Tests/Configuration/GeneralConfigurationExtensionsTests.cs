using JungleBus.Configuration;
using JungleBus.Interfaces.Configuration;
using JungleBus.Interfaces.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JungleBus.Tests.Configuration
{
    [TestClass]
    public class GeneralConfigurationExtensionsTests
    {
        [TestMethod]
        [ExpectedException(typeof(JungleBusConfigurationException))]
        public void GeneralConfigurationExtensionsTests_UsingJsonSerialization_NullConfiguration()
        {
            (null as IConfigureMessageSerializer).UsingJsonSerialization();
        }

        [TestMethod]
        [ExpectedException(typeof(JungleBusConfigurationException))]
        public void GeneralConfigurationExtensionsTests_UsingJsonSerialization_NoObjectBuilder()
        {
            BusConfiguration configuration = new BusConfiguration();
            (configuration as IConfigureMessageSerializer).UsingJsonSerialization();
        }

        [TestMethod]
        [ExpectedException(typeof(JungleBusConfigurationException))]
        public void GeneralConfigurationExtensionsTests_WithObjectBuilder_NullConfiguration()
        {
            (null as IConfigureObjectBuilder).WithObjectBuilder(null);
        }

        [TestMethod]
        [ExpectedException(typeof(JungleBusConfigurationException))]
        public void GeneralConfigurationExtensionsTests_WithObjectBuilder_NullObjectBuilder()
        {
            BusConfiguration configuration = new BusConfiguration();
            (configuration as IConfigureObjectBuilder).WithObjectBuilder(null);
        }

        [TestMethod]
        [ExpectedException(typeof(JungleBusConfigurationException))]
        public void GeneralConfigurationExtensionsTests_EnableMessageLogging_NullConfiguration()
        {
            (null as IBusConfiguration).EnableMessageLogging();
        }

        [TestMethod]
        [ExpectedException(typeof(JungleBusConfigurationException))]
        public void GeneralConfigurationExtensionsTests_CreateStartableBus_NullConfiguration()
        {
            (null as IBusConfiguration).CreateStartableBus();
        }

        [TestMethod]
        [ExpectedException(typeof(JungleBusConfigurationException))]
        public void GeneralConfigurationExtensionsTests_CreateStartableBus_NullObjectBuilder()
        {
            BusConfiguration configuration = new BusConfiguration();
            configuration.ObjectBuilder = null;
            configuration.CreateStartableBus();
        }
    }
}
