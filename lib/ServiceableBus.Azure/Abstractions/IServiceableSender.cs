using Azure.Messaging.ServiceBus;
using ServiceableBus.Contracts;

namespace ServiceableBus.Azure.Abstractions;
public interface IServiceableSender : IDisposable
{
    public Type EventType { get; }
    void Initialise(ServiceBusClient client);
    Task SendAsync(IServiceableBusEvent message);
}