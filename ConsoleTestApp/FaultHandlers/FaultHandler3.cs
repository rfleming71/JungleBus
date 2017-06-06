using System;
using Common.Logging;
using JungleBus.Interfaces;
using JungleBus.Messaging;
using JungleBus.Queue.Messaging;

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

        public void Handle(TransportMessage message, Exception ex)
        {
            _log.Info("Starting message fault Handler 3");
        }
    }
}
