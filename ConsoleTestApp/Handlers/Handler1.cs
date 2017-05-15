using System.Threading;
using Common.Logging;
using JungleBus.Interfaces;
using Messages;

namespace ConsoleTestApp.Handlers
{
    public class Handler1 : IHandleMessage<TestMessage>
    {
        private readonly IBus _bus;
        private readonly ILog _log;
        public Handler1(/*IBus bus,*/ ILog log)
        {
            /*_bus = bus;*/
            _log = log;
        }

        public void Handle(TestMessage message)
        {
            _log.Info("Starting message Handler 1");
            Thread.Sleep(5000);
            _log.Info("Finished message Handler 1");
        }
    }
}
