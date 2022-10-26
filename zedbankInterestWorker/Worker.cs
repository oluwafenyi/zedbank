using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Quartz;
using zedbankInterestWorker.Contracts;
using zedbankInterestWorker.Schedule;

namespace zedbankInterestWorker
{
    public class Worker: BackgroundService
    {
        private readonly IBus _bus;

        public Worker(IBus bus)
        {
            _bus = bus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // while (!stoppingToken.IsCancellationRequested)
            // {
            var schedulerEndpoint = await _bus.GetSendEndpoint(new Uri("queue:quartz"));
            await schedulerEndpoint.ScheduleRecurringSend(new Uri("queue:everyday-recurring-schedule"),
                new EverydayRecurringSchedule(), new EverydayRecurringScheduleElapsed(), stoppingToken);
            // }

            // if (scheduledRecurringMessage != null)
            // {
            //     await _bus.CancelScheduledRecurringSend(scheduledRecurringMessage);
            // }
        }
    }
}