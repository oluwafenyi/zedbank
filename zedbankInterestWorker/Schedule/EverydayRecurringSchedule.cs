using MassTransit.Scheduling;
using Quartz;

namespace zedbankInterestWorker.Schedule
{
    public class EverydayRecurringSchedule: DefaultRecurringSchedule
    {
        public EverydayRecurringSchedule()
        {
            CronExpression = "*/10 * * * * ?";
        }
    }
}