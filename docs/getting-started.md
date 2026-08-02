# Getting started

## Prerequisites

- .NET 9
- An Azure Service Bus namespace (queues and/or topics + subscriptions)
- Connection string (or equivalent access) for that namespace

## Install

Add the Azure package (and Contracts if you want them as a direct reference):

```bash
dotnet add package ServiceableBus.Azure
dotnet add package ServiceableBus.Contracts
```

`Azure.Messaging.ServiceBus` and ASP.NET Core hosting/DI come in transitively via `ServiceableBus.Azure`.

## Configure

In `appsettings.json` (or environment / user secrets):

```json
{
  "ServiceableBus": {
    "ConnectionString": "Endpoint=sb://YOUR_NAMESPACE.servicebus.windows.net/;SharedAccessKeyName=...;SharedAccessKey=..."
  }
}
```

Prefer user secrets or environment variables for real credentials. Do not commit live connection strings.

## Define an event

```csharp
using ServiceableBus.Azure;
using ServiceableBus.Contracts;

public class TestEvent : ServiceableBusEvent<TestEvent.TestEventPayload>
{
    public const string Queue = "test-event";

    public record TestEventPayload(string Field1, int Field2, int Field3) : IServiceableBusPayload;
}
```

## Define a handler

```csharp
using ServiceableBus.Contracts;

public class TestEventServiceBusHandler : IServiceableBusEventHandler<TestEvent>
{
    public Task Handle(TestEvent @event, ServiceablePropertyBag properties)
    {
        // handle @event.Payload and properties.Properties
        return Task.CompletedTask;
    }
}
```

## Register in `Program.cs`

Order matters: register listeners, handlers, and senders **before** `AddServiceableBus()`.

```csharp
using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Azure.Extensions;
using ServiceableBus.Azure.Options;
using ServiceableBus.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions();

// Listeners
builder.AddServiceableBusQueueListener(() =>
    new ServiceableQueueListenerOptions<TestEvent>
    {
        QueueName = TestEvent.Queue
    });

builder.AddServiceableBusTopicListener(() =>
    new ServiceableTopicListenerOptions<TestTopicEvent>
    {
        TopicName = TestTopicEvent.Topic,
        SubscriptionName = "your-subscription-name"
    });

// Handlers
builder.RegisterServiceableBusHandler<TestEvent, TestEventServiceBusHandler>();
builder.RegisterServiceableBusHandler<TestTopicEvent, TestTopicEventServiceBusHandler>();

// Senders (one registration per event type you will publish)
builder.AddServiceableBusEventSender<TestEvent>(TestEvent.Queue);
builder.AddServiceableBusEventSender<TestTopicEvent>(TestTopicEvent.Topic);

builder.AddServiceableBus();

var app = builder.Build();
app.Run();
```

Namespaces used by the library today:

- Extensions: `ServiceableBus.Azure.Extensions`
- Abstractions (e.g. `IServiceablePublisher`): `ServiceableBus.Azure.Abstractions`
- Options: `ServiceableBus.Azure.Options`
- Contracts: `ServiceableBus.Contracts`

See also [Publishing](./publishing.md) and [Receiving](./receiving.md).
