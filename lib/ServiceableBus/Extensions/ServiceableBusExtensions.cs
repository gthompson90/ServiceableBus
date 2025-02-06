using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ServiceableBus.Azure;
using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Azure.Listeners;
using ServiceableBus.Azure.Options;
using ServiceableBus.Contracts;

namespace ServiceableBus.Extensions;

public static class ServiceableBusExtensions
{
    public static WebApplicationBuilder AddServiceableBus(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<ServiceableBusOptions>(builder.Configuration.GetSection("ServiceableBus"));
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

    public static WebApplicationBuilder AddServiceableBusTopicListener<T>(this WebApplicationBuilder builder, string queueName)
        where T : IServiceableBusEvent
    {
        builder.Services.AddSingleton<IServiceableQueueListenerOptions<T>>(sp => new ServiceableQueueListenerOptions<T>(queueName));
        builder.Services.AddSingleton<IServiceableListener, ServiceableQueueListener<T>>();
        return builder;
    }
}