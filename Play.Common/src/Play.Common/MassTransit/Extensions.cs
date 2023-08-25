using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Play.Common.Settings;

namespace Play.Common.MassTransit
{
    public static class Extensions
    {
        public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services)
        {

            services.AddMassTransit(configure =>
            {
                configure.UsingRabbitMq((context, configurator) =>
                {
                    configure.AddConsumers(Assembly.GetEntryAssembly());
                    var Configuration = context.GetService<IConfiguration>();
                    var serviceSettings = Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                    var rabbitmqsettings = Configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
                    configurator.Host(rabbitmqsettings.Host);
                    configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));
                });
            });
            return services;
        }
    }

}