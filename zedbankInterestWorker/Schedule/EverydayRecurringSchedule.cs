using MassTransit.Scheduling;

namespace zedbankInterestWorker.Schedule
{
    public class EverydayRecurringSchedule: DefaultRecurringSchedule
    {
        public EverydayRecurringSchedule()
        {
            // CronExpression = "0 0/1 * 1/1 * ? *";  // Every minute: for testing
            CronExpression = "0 0 0 * * ? *";  // Every day by 12:00am
        }
    }
}