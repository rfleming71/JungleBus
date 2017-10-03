using System;
using System.Threading.Tasks;
using Common.Logging;
using JungleBus.Interfaces;
using JungleBus.Messaging;
using JungleQueue.Messaging;

namespace ConsoleTestApp.FaultHandlers
{
    public class FaultHandler3 : IHandleMessageFaults<TransportMessage>
    {
        private readonly IBus _bus;
        private readonly ILog _log;
        public FaultHandler3(IBus bus, ILog log)
        {
            _bus = bus;
            _log = log;
        }

        public Task Handle(TransportMessage message, Exception ex)
        {
            _log.Info("Starting message fault Handler 3");
            return Task.CompletedTask;
        }
    }
}
