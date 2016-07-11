using System;
using JungleBus.Configuration;
using JungleBus.Exceptions;
using JungleBus.Interfaces.Serialization;
using JungleBus.IoC;
using JungleBus.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StructureMap;

namespace JungleBus.Tests.Configuration
{
    [TestClass]
    public class GeneralConfigurationExtensionsTests
    {
        [TestMethod]
        public void GeneralConfigurationExtensionsTests_WithStructureMapObjectBuilder_NoContainer()
        {
            BusConfiguration configuration = new BusConfiguration();
            (configuration as IConfigureObjectBuilder).WithStructureMapObjectBuilder();
            Assert.IsNotNull(configuration.ObjectBuilder);
        }

        [TestMethod]
        [ExpectedException(typeof(JungleBusConfigurationException))]
        public void GeneralConfigurationExtensionsTests_WithStructureMapObjectBuilder_NoContainer_NullConfiguration()
        {
            (null as IConfigureObjectBuilder).WithStructureMapObjectBuilder();
        }
        [TestMethod]
        public void GeneralConfigurationExtensionsTests_WithStructureMapObjectBuilder_NewContainer()
        {
            Container newContainer = new Container();
            newContainer.Configure(x => { x.For<string>().Use<string>("Testing"); });
            BusConfiguration configuration = new BusConfiguration();
            (configuration as IConfigureObjectBuilder).WithStructureMapObjectBuilder(newContainer);
            Assert.IsNotNull(configuration.ObjectBuilder);
            string configureString = configuration.ObjectBuilder.GetValue<string>();
            Assert.AreEqual("Testing", configureString);
        }

        [TestMethod]
        [ExpectedException(typeof(JungleBusConfigurationException))]
        public void GeneralConfigurationExtensionsTests_WithStructureMapObjectBuilder_NewContainer_NullConfiguration()
        {
            Container newContainer = new Container();
            (null as IConfigureObjectBuilder).WithStructureMapObjectBuilder(newContainer);
        }

        [TestMethod]
        public void GeneralConfigurationExtensionsTests_UsingJsonSerialization()
        {
            Container newContainer = new Container();
            BusConfiguration configuration = new BusConfiguration();
            configuration.ObjectBuilder = new StructureMapObjectBuilder(newContainer);
            (configuration as IConfigureMessageSerializer).UsingJsonSerialization();
            Assert.IsNotNull(configuration.ObjectBuilder);
            IMessageSerializer serializer = newContainer.TryGetInstance<IMessageSerializer>();
            Assert.IsNotNull(serializer);
        }

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
