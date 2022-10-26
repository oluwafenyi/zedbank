using MassTransit;

namespace zedbankInterestWorker.Consumers
{
    public class ZedbankInterestWorkerConsumerDefinition :
        ConsumerDefinition<ZedbankInterestWorkerConsumer>
    {
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<ZedbankInterestWorkerConsumer> consumerConfigurator)
        {
            endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));
        }
    }
}