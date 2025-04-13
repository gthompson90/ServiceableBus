using Azure.Messaging.ServiceBus;
using ServiceableBus.Azure.Options;

namespace ServiceableBus.Azure.Abstractions;

public interface IServiceableBusClientFactory
{
    public ServiceBusSender CreateSender(IServiceablePublisherOptions options);

    public ServiceBusSender CreateDeferralNotificationSender(ServiceableBusOptions options);
}