using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using zedbankInterestWorker.Contracts;

namespace zedbankInterestWorker.Consumers
{
    public class ZedbankInterestWorkerConsumer :
        IConsumer<ZedbankInterestWorker>
    {
        private readonly ILogger<ZedbankInterestWorkerConsumer> _logger;

        public ZedbankInterestWorkerConsumer(ILogger<ZedbankInterestWorkerConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<ZedbankInterestWorker> context)
        {
            _logger.LogInformation("Received Task: {Text}", context.Message.Value);
            return Task.CompletedTask;
        }
    }
}