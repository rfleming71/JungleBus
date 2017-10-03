using System;
using System.Threading.Tasks;
using JungleQueue.Interfaces.Statistics;

namespace ConsoleTestApp
{
    class StatsTracker : IWantMessageStatistics
    {
        public Task ReceiveStatisitics(IMessageStatistics statistics)
        {
            Console.WriteLine("Type: {0} - Successful: {1} - Runtime: {2}", statistics.MessageType, statistics.Success, statistics.HandlerRunTime.TotalMilliseconds);
            return Task.CompletedTask;
        }
    }
}
