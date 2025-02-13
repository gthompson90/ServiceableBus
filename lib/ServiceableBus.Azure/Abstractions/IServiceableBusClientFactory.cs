using Azure.Messaging.ServiceBus;

namespace ServiceableBus.Azure.Abstractions
{
    public interface IServiceableBusClientFactory
    {
        public ServiceBusSender CreateSender(IServiceablePublisherOptions options);
    }
}
