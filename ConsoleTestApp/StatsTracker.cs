using System;
using JungleBus.Interfaces.Statistics;

namespace ConsoleTestApp
{
    class StatsTracker : IWantMessageStatistics
    {
        public void ReceiveStatisitics(IMessageStatistics statistics)
        {
            Console.WriteLine("Type: {0} - Successful: {1} - Runtime: {2}", statistics.MessageType, statistics.Success, statistics.HandlerRunTime.TotalMilliseconds);
        }
    }
}
