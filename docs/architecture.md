# Architecture

## Packages

| Project | Role |
|---------|------|
| `ServiceableBus.Contracts` | Shared contracts: `IServiceableBusEvent`, `IServiceableBusPayload`, `IServiceableBusEventHandler<T>`, `ServiceablePropertyBag` |
| `ServiceableBus.Azure` | Azure Service Bus implementation: DI extensions, publisher, queue/topic listeners, hosted service, options |

NuGet package IDs match the project names (`ServiceableBus.Azure`, `ServiceableBus.Contracts`) unless overridden in the `.csproj`.

Target framework: **.NET 9**. The Azure package takes a framework reference on `Microsoft.AspNetCore.App` because registration extensions hang off `WebApplicationBuilder`.

## Component map

```
WebApplicationBuilder
  ├─ AddServiceableBusQueueListener<T> / AddServiceableBusTopicListener<T>
  │    → registers IServiceableListener (+ typed options)
  ├─ RegisterServiceableBusHandler<TEvent, THandler>
  │    → scoped IServiceableBusEventHandler<TEvent>
  ├─ AddServiceableBusEventSender<T>
  │    → IServiceablePublisherOptions (queue/topic name + message type)
  └─ AddServiceableBus()
       → ServiceableBusOptions (config)
       → IServiceableBusClientFactory (senders)
       → IServiceablePublisher
       → ServiceableBusBackgroundService (starts all IServiceableListener)

Publish path:
  IServiceablePublisher.PublishAsync<T>
    → resolve IServiceablePublisherOptions for T
    → ServiceBusSender via client factory
    → JSON body (System.Text.Json) + optional ApplicationProperties

Receive path:
  ServiceableBusBackgroundService
    → shared ServiceBusClient
    → ServiceableQueueListener<T> / ServiceableTopicListener<T>
    → deserialize → scoped handler.Handle(event, properties)
    → Complete or DeadLetter
```

## Event model

Recommended shape (used by the sample):

- Event type inherits `ServiceableBusEvent<TPayload>` (`MessageTypeName`, `CreatedAt`, `Payload`)
- Payload implements `IServiceableBusPayload`
- Handler implements `IServiceableBusEventHandler<TEvent>` with  
  `Task Handle(TEvent @event, ServiceablePropertyBag properties)`

`IServiceableBusEvent` itself is an empty marker; listeners/publisher only constrain on that interface. The abstract `ServiceableBusEvent<T>` base is optional but is the intended envelope.

## Configuration

`AddServiceableBus` binds `IOptions<ServiceableBusOptions>` from configuration section `ServiceableBus`:

```json
{
  "ServiceableBus": {
    "ConnectionString": "<Azure Service Bus connection string>"
  }
}
```

Queue/topic names and subscription names are not read from config; they are supplied in code when registering listeners and senders.

## Hosting model

- Registration API is ASP.NET Core–centric (`WebApplicationBuilder` extensions).
- Processing runs inside `IHostedService` (`ServiceableBusBackgroundService`).
- Handlers are resolved from a **scoped** DI scope per message.
- Publisher and client factory are **singletons**.
