# ServiceableBus

ServiceableBus is a .NET 9 library for publishing and handling Azure Service Bus messages with typed events, handlers, and DI registration.

## Documentation

See [`/docs`](./docs/README.md) for architecture, getting started, publishing, receiving, and internal engineering review notes.

## Prerequisites

- .NET 9
- An Azure Service Bus namespace and connection string

## Installation

```bash
dotnet add package ServiceableBus.Azure
dotnet add package ServiceableBus.Contracts
```

`Azure.Messaging.ServiceBus` is brought in by `ServiceableBus.Azure`.

## Configuration

Add settings to `appsettings.json` (prefer user secrets or environment variables for real credentials):

```json
{
  "ServiceableBus": {
    "ConnectionString": "YourAzureServiceBusConnectionString"
  }
}
```

### Event

```csharp
using ServiceableBus.Azure;
using ServiceableBus.Contracts;

public class TestEvent : ServiceableBusEvent<TestEvent.TestEventPayload>
{
    public const string Queue = "test-event";

    public record TestEventPayload(string Field1, int Field2, int Field3) : IServiceableBusPayload;
}
```

### Handler

```csharp
using ServiceableBus.Contracts;

public class TestEventServiceBusHandler : IServiceableBusEventHandler<TestEvent>
{
    public Task Handle(TestEvent @event, ServiceablePropertyBag properties)
    {
        Console.WriteLine($"Handling event: {@event.Payload}");
        return Task.CompletedTask;
    }
}
```

## Usage

Register listeners, handlers, and senders **before** `AddServiceableBus()`.

```csharp
using ServiceableBus.Azure.Abstractions;
using ServiceableBus.Azure.Extensions;
using ServiceableBus.Azure.Options;
using ServiceableBus.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions();

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

builder.RegisterServiceableBusHandler<TestEvent, TestEventServiceBusHandler>();
builder.RegisterServiceableBusHandler<TestTopicEvent, TestTopicEventServiceBusHandler>();

builder.AddServiceableBusEventSender<TestEvent>(TestEvent.Queue);
builder.AddServiceableBusEventSender<TestTopicEvent>(TestTopicEvent.Topic);

builder.AddServiceableBus();

var app = builder.Build();
app.Run();
```

### Publishing

Resolve `IServiceablePublisher` and call `PublishAsync`. The public interface requires a `ServiceablePropertyBag` factory (use an empty bag if you have no application properties):

```csharp
var sender = app.Services.GetRequiredService<IServiceablePublisher>();

await sender.PublishAsync(
    new TestEvent
    {
        MessageTypeName = "TestEvent",
        CreatedAt = DateTime.UtcNow,
        Payload = new TestEvent.TestEventPayload("Test", 1, 4)
    },
    () => new ServiceablePropertyBag
    {
        Properties =
        [
            ("AppName", "TestApp"),
            ("AnotherProperty", "WhateverValueYouLike")
        ]
    });
```

## ServiceablePropertyBag

`ServiceablePropertyBag` holds application properties sent with the Service Bus message and reconstituted on receive.

```csharp
var propertyBag = new ServiceablePropertyBag
{
    Properties =
    [
        ("Property1", "Value1"),
        ("Property2", 123),
        ("Property3", true)
    ]
};

await sender.PublishAsync(message, () => propertyBag);
```

## Additional information

- Set `ServiceableBus:ConnectionString` correctly before running listeners or publishers.
- You can register multiple listeners and handlers using the same pattern.
- A runnable sample is under `samples/ServiceableBus.Azure.Sample.Api`.

## License

This project is licensed under the MIT License.

## Contributing

Contributions are welcome. Please open an issue or submit a pull request.

## Contact

For questions or issues, open an issue on the GitHub repository.
