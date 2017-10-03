using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using JungleBus.Interfaces;
using Messages;

namespace ConsoleTestApp.FaultHandlers
{
    public class FaultHandler1 : IHandleMessageFaults<TestMessage>
    {
        private readonly IBus _bus;
        private readonly ILog _log;
        public FaultHandler1(IBus bus, ILog log)
        {
            _bus = bus;
            _log = log;
        }

        public Task Handle(TestMessage message, Exception ex)
        {
            _log.Info("Starting message fault Handler 1");
            return Task.CompletedTask;
        }
    }
}
