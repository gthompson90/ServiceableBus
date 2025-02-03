using System.Reflection.Metadata.Ecma335;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ServiceableBus
{
    public static class ServiceableBusExtensions
    {
        public static WebApplicationBuilder AddServiceableBus(this WebApplicationBuilder builder, string connectionString)
        {
            builder.Services.AddHostedService(sp =>
                new ServiceableBusBackgroundService(connectionString, sp));

            return builder;
        }

        public static WebApplicationBuilder RegisterServiceableBusHandler<T, X>(this WebApplicationBuilder builder)
            where T : ServiceableBusEvent
            where X : class, IServiceableBusEventHandler<T> 
        {
            var key = "Service1";
            builder.Services.AddKeyedScoped<IServiceableBusEventHandler<T>, X>(key);
            return builder;
        }

        //public static WebApplication AddServiceableBusTopicListener<T>(this WebApplication app, string topicName) where T : ServiceableBusEvent
        //{
        //    var service = app.Services.GetRequiredService<ServiceableBusBackgroundService>();
        //    service.AddTopic<T>(topicName);
        //    return app;
        //}
    }
}
