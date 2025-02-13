using Azure.Messaging.ServiceBus;
using ServiceableBus.Contracts;

namespace ServiceableBus.Azure.Abstractions
{
    public interface IServiceableBusClientFactory
    {
        public ServiceBusSender CreateSender(IServiceablePublisherOptions options);
    }
}
