using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace zedbankInterestWorker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddQuartz(q =>
                    {
                        q.UseMicrosoftDependencyInjectionJobFactory();
                    });
                    
                    services.AddMassTransit(x =>
                    {
                        x.SetKebabCaseEndpointNameFormatter();

                        // By default, sagas are in-memory, but should be changed to a durable
                        // saga repository.
                        x.SetInMemorySagaRepositoryProvider();

                        var entryAssembly = Assembly.GetEntryAssembly();

                        x.AddPublishMessageScheduler();
                        x.AddQuartzConsumers();

                        x.AddConsumers(entryAssembly);
                        x.AddSagaStateMachines(entryAssembly);
                        x.AddSagas(entryAssembly);
                        x.AddActivities(entryAssembly);

                        x.UsingRabbitMq((context, cfg) =>
                        {
                            var config = ConfigurationManager.Config;
                            cfg.Host(config["RabbitMq:HOST"], "/", h =>
                            {
                                h.Username(config["RabbitMq:USERNAME"]);
                                h.Password(config["RabbitMq:PASSWORD"]);
                            });
                            
                            cfg.UsePublishMessageScheduler();
                            cfg.ConfigureEndpoints(context);
                        });
                    });
                    
                    services.AddHostedService<Worker>();
                });
    }
}
