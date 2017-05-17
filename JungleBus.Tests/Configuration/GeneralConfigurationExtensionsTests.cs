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
    }
}
