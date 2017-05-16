﻿using System;
using Common.Logging;
using JungleBus.Interfaces;
using Messages;

namespace ConsoleTestApp.FaultHandlers
{
    public class FaultHandler2 : IHandleMessageFaults<TestMessage>
    {
        private readonly IBus _bus;
        private readonly ILog _log;
        public FaultHandler2(IBus bus, ILog log)
        {
            _bus = bus;
            _log = log;
        }

        public void Handle(TestMessage message, Exception ex)
        {
            _log.Info("Starting message fault Handler 2");
        }
    }
}
