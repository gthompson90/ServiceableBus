using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Azure.Options;

namespace ServiceableBus.Azure;

internal class ServiceableBusClientFactory : IServiceableBusClientFactory
{
    private readonly ServiceableBusOptions _serviceableBusOptions;
    private readonly ServiceBusClient _client;

    public ServiceableBusClientFactory(IOptions<ServiceableBusOptions> serviceableBusOptions)
    {
        _serviceableBusOptions = serviceableBusOptions.Value;
        _client = new ServiceBusClient(_serviceableBusOptions.ConnectionString);
    }

    public ServiceBusSender CreateSender(IServiceablePublisherOptions options)
    {
        var sender = _client.CreateSender(options.QueueName);
        return sender;
    }
}