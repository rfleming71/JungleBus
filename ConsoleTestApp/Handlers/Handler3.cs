using JungleBus;
using Common.Logging;
using Messages;

namespace ConsoleTestApp.Handlers
{
    public class Handler3 : IHandleMessage<TestMessage2>
    {
        private readonly ILog _log;
        public Handler3(ILog log)
        {
            _log = log;
        }

        public void Handle(TestMessage2 message)
        {
            _log.Info("Handling message Handler 3");
        }
    }
}
