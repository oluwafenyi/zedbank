using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using zedbankInterestWorker.Contracts;

namespace zedbankInterestWorker.Consumers
{
    public class EverydayRecurringScheduleConsumer: IConsumer<EverydayRecurringScheduleElapsed>
    {
        private readonly ILogger<EverydayRecurringScheduleElapsed> _logger;

        public EverydayRecurringScheduleConsumer(ILogger<EverydayRecurringScheduleElapsed> logger)
        {
            _logger = logger;
        }
        
        public Task Consume(ConsumeContext<EverydayRecurringScheduleElapsed> context)
        {
            _logger.LogInformation("here");
            return Task.CompletedTask;
        }
    }
}