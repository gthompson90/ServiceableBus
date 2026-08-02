# Publishing

## Registration

For each event type you publish, register a sender with the destination queue or topic name:

```csharp
builder.AddServiceableBusEventSender<TestEvent>(TestEvent.Queue);
// or a topic:
builder.AddServiceableBusEventSender<TestTopicEvent>(TestTopicEvent.Topic);
```

`AddServiceableBus()` registers a singleton `IServiceablePublisher`. Publishing a type that was never registered throws:

`InvalidOperationException`: no publisher options for that message type.

## API

Public interface (`IServiceablePublisher`):

```csharp
Task PublishAsync<T>(T message, Func<ServiceablePropertyBag> action)
    where T : IServiceableBusEvent;
```

Resolve and publish after the app is built (or from a service that injects `IServiceablePublisher`):

```csharp
var publisher = app.Services.GetRequiredService<IServiceablePublisher>();

await publisher.PublishAsync(
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

### Application properties (`ServiceablePropertyBag`)

- Property on the bag is `Properties` (tuple array of `(string Key, object Value)`).
- Values must be types Azure Service Bus allows in `ApplicationProperties` (string, number, bool, DateTime, Guid, etc.).
- On receive, the same bag is rebuilt from `ApplicationProperties` and passed into `Handle`.

If you do not need custom properties, still pass a factory that returns an empty bag when calling through `IServiceablePublisher` (the public interface requires the second argument):

```csharp
await publisher.PublishAsync(message, () => new ServiceablePropertyBag { Properties = [] });
```

## Wire format

- Body: UTF-8 JSON via **System.Text.Json**
- `ContentType`: `application/json`
- `CorrelationId`: new GUID per publish
- Serializer options include string enums (camelCase), `IncludeFields`, `PropertyNameCaseInsensitive`, indented JSON

Note: `ServiceableBusEvent<T>` attributes use **Newtonsoft** `[JsonProperty]`. The publisher/listeners serialize with System.Text.Json, so those attributes are not applied on the wire path. Property names follow System.Text.Json defaults (typically PascalCase unless you customize options elsewhere).
