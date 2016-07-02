using System;
using JungleBus.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JungleBus.Tests.Serialization
{
    [TestClass]
    public class JsonNetSerializerTests
    {
        [TestMethod]
        public void JsonNetSerializerTests_CanDeserialize_Serialized_Messages()
        {
            JsonNetSerializer serializer = new JsonNetSerializer();
            TestMessage message = new TestMessage()
            {
                ID = 1,
                Modified = DateTime.Now,
                Name = "Name",
                Price = 123.12,
            };

            TestMessage actualMessage = (TestMessage)serializer.Deserialize(serializer.Serialize(message), typeof(TestMessage));
            Assert.IsNotNull(actualMessage);
            Assert.AreEqual(message.ID, actualMessage.ID);
            Assert.AreEqual(message.Modified, actualMessage.Modified);
            Assert.AreEqual(message.Name, actualMessage.Name);
            Assert.AreEqual(message.Price, actualMessage.Price);
        }
    }
}
