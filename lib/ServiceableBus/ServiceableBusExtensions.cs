using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceableBus;

public static class ServiceableBusExtensions
{
    public static WebApplicationBuilder AddServiceableBus(this WebApplicationBuilder builder, string connectionString)
    {
        builder.Services.AddSingleton<IServiceableBusOptions>(new ServiceableBusOptions(connectionString));
        builder.Services.AddHostedService<ServiceableBusBackgroundService>();

        return builder;
    }

    public static WebApplicationBuilder RegisterServiceableBusHandler<T, X>(this WebApplicationBuilder builder)
        where T : IServiceableBusEvent
        where X : class, IServiceableBusEventHandler<T> 
    {
        builder.Services.AddScoped<IServiceableBusEventHandler<T>, X>();
        return builder;
    }

    public static WebApplicationBuilder AddServiceableBusTopicListener<T>(this WebApplicationBuilder builder, string queueName) where T : IServiceableBusEvent
    {
        builder.Services.AddSingleton<IServiceableQueueListenerOptions<T>>(sp => new ServiceableQueueListenerOptions<T>(queueName));
        builder.Services.AddSingleton<IServiceableListener, ServiceableQueueListener<T>>();
        return builder;
    }
}