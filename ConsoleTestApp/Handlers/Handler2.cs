using System.Threading;
using JungleBus;
using Common.Logging;
using Messages;

namespace ConsoleTestApp.Handlers
{
    public class Handler2 : IHandleMessage<TestMessage>
    {
        private readonly IBus _bus;
        private readonly ILog _log;
        public Handler2(IBus bus, ILog log)
        {
            _bus = bus;
            _log = log;
        }

        public void Handle(TestMessage message)
        {
            _log.Info("Starting message Handler 2");
            Thread.Sleep(10000);
            _bus.PublishLocal<TestMessage2>(x =>
            {
                x.ID = 1;
                x.Modified = 2;
            });

            _log.Info("Published TestMessage2");
            _log.Info("Finished message Handler 2");
        }
    }
}
