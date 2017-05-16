using System;
using Amazon;
using JungleBus.Configuration;
using JungleBus.Interfaces;
using JungleBus.Interfaces.Statistics;
using JungleBus.StructureMap;
using Messages;
using StructureMap;

namespace ConsoleTestApp
{
    class Program
    {
        private static IContainer _container;

        static void Main(string[] args)
        {
            _container = new Container();
            _container.Configure(x =>
            {
                x.For<IWantMessageStatistics>().Use<StatsTracker>();
            });

            bool testFullBus = true;
            IRunJungleBus bus;
            IBus sendBus;
            if (testFullBus)
            {
                bus = CreateFullBus();
                sendBus = bus.CreateSendBus();
            }
            else
            {
                bus = CreateRecieveOnlyBus();
                sendBus = CreateSendOnlyBus();
            }

            bus.StartReceiving();

            do
            {
                Console.WriteLine("Press any key to send a message");
                sendBus.Publish<TestMessage>(x =>
                {
                    x.ID = 123;
                    x.Modified = DateTime.Now;
                    x.Name = Guid.NewGuid().ToString();
                });
            }
            while (Console.ReadKey(true).Key != ConsoleKey.Escape);

            bus.StopReceiving();
        }

        static IRunJungleBus CreateRecieveOnlyBus()
        {
            return BusBuilder.Create("jb")
                .WithStructureMapObjectBuilder(_container)
                .UsingJsonSerialization()
                .EnableMessageLogging()
                .SetInputQueue("Test_Queue1", RegionEndpoint.USEast1)
                .SetSqsPollWaitTime(14)
                .UsingEventHandlersFromEntryAssembly()
                .SetNumberOfPollingInstances(1)
                .PublishingLocalEventsOnly()
                .CreateStartableBus();
        }

        static IBus CreateSendOnlyBus()
        {
            return BusBuilder.Create("jb")
                .WithStructureMapObjectBuilder(_container)
                .UsingJsonSerialization()
                .EnableMessageLogging()
                .PublishingMessages(typeof(TestMessage).Assembly.ExportedTypes, RegionEndpoint.USEast1)
                .CreateSendOnlyBusFactory()();
        }

        static IRunJungleBus CreateFullBus()
        {
            return BusBuilder.Create("jb")
                .WithStructureMapObjectBuilder(_container)
                .UsingJsonSerialization()
                .EnableMessageLogging()
                .PublishingMessages(typeof(TestMessage).Assembly.ExportedTypes, RegionEndpoint.USEast1)
                .SetInputQueue("Test_Queue1", RegionEndpoint.USEast1)
                .SetSqsPollWaitTime(14)
                .UsingEventHandlersFromEntryAssembly()
                .SetNumberOfPollingInstances(1)
                .CreateStartableBus();
        }
    }
}
