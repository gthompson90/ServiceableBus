# Receiving

## Queue listener

```csharp
builder.AddServiceableBusQueueListener(() =>
    new ServiceableQueueListenerOptions<TestEvent>
    {
        QueueName = TestEvent.Queue
    });

builder.RegisterServiceableBusHandler<TestEvent, TestEventServiceBusHandler>();
```

Creates a `ServiceBusProcessor` for the given queue and routes messages to `IServiceableBusEventHandler<TestEvent>`.

## Topic listener

```csharp
builder.AddServiceableBusTopicListener(() =>
    new ServiceableTopicListenerOptions<TestTopicEvent>
    {
        TopicName = TestTopicEvent.Topic,
        SubscriptionName = "your-subscription-name"
    });

builder.RegisterServiceableBusHandler<TestTopicEvent, TestTopicEventServiceBusHandler>();
```

Creates a processor for `topic` + `subscription`.

## Handler contract

```csharp
public interface IServiceableBusEventHandler<T> where T : IServiceableBusEvent
{
    Task Handle(T @event, ServiceablePropertyBag properties);
}
```

Handlers are registered as **scoped** and resolved inside a new scope per message.

## Processing semantics (current behavior)

For each message the listener:

1. Deserializes the body to `T` with System.Text.Json (same options as publish).
2. If deserialization yields a non-null instance, resolves the handler and invokes `Handle` (via reflection).
3. On success path: **completes** the message.
4. On exception: logs to `Console`, then **dead-letters** the message.

Implications to be aware of:

- If deserialize returns `null`, the message is still **completed** (no handler call, no dead-letter).
- There is no abandon / retry configuration exposed; processor options use SDK defaults.
- Errors in the processor host callback are written with `Console.WriteLine` only (no `ILogger`).

## Lifecycle

`ServiceableBusBackgroundService`:

- On start: starts every registered `IServiceableListener` with one shared `ServiceBusClient` built from `ServiceableBus:ConnectionString`.
- On stop: stops processors and disposes listeners, then (on dispose) disposes the client.

Register all listeners before `AddServiceableBus()` so they are present when the hosted service is constructed.
