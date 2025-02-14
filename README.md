# ServiceableBus

ServiceableBus is a .NET library for handling Azure Service Bus messages with ease. This guide will help you set up and use the ServiceableBus package in your project.

## Prerequisites

- .NET 9
- Azure Service Bus namespace and connection string

## Installation

1. Add the `ServiceableBus` package to your project. You can do this via the NuGet Package Manager or by running the following command in the terminal:

```
dotnet add package ServiceableBus
```

    
2. Ensure you have the necessary dependencies in your project:

```
dotnet add package Azure.Messaging.ServiceBus
dotnet add package Microsoft.Extensions.DependencyInjection
```

## Configuration

1. Add the necessary configuration settings to your `appsettings.json` file:
```
{
  "ServiceableBus": {
    "ConnectionString": "YourAzureServiceBusConnectionString"
  }
}
```
2. Create your event class implementing `IServiceableBusEvent`:
```
public class TestEvent : IServiceableBusEvent
{
    public const string Topic = "test-topic";
    public string Property { get; set; }
}
```
3. Create your event handler class implementing `IServiceableBusEventHandler<T>`:
```
public class TestEventServiceBusHandler : IServiceableBusEventHandler<TestEvent>
{
    public Task Handle(TestEvent @event)
    {
        Console.WriteLine($"Handling event: {@event.Property}");
        return Task.CompletedTask;
    }
}
```
## Usage

1. In your `Program.cs` file, configure the `ServiceableBus`:
```
using ServiceableBus.Extensions;
using ServiceableBus.Sample.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Adding options is required for the ServiceableBus configuration.
builder.Services.AddOptions();

//Add your Listeners and Handlers here BEFORE adding the ServiceableBus.
builder.AddServiceableBusQueueListener(() =>
    new ServiceableQueueListenerOptions<TestEvent>()
    { 
        QueueName = TestEvent.Queue 
    });

builder.AddServiceableBusTopicListener(() =>
    new ServiceableTopicListenerOptions<TestTopicEvent>()
    { 
        SubscriptionName =  appTopicSubscriptionName,
        TopicName = TestTopicEvent.Topic
    });

builder.RegisterServiceableBusHandler<TestEvent, TestEventServiceBusHandler>();
builder.RegisterServiceableBusHandler<TestTopicEvent, TestTopicEventServiceBusHandler>();

builder.AddServiceableBus();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
```
2. Run your application:
```
dotnet run
```

3. Adding a Sender for TestEvent and sending a message:
```
using ServiceableBus.Extensions;
using ServiceableBus.Sample.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Adding options is required for the ServiceableBus configuration.
builder.Services.AddOptions();

// Add your Listeners, Handlers and Senders here BEFORE adding the ServiceableBus.
builder.AddServiceableBusQueueListener(() =>
    new ServiceableQueueListenerOptions<TestEvent>()
    { 
        QueueName = TestEvent.Queue 
    });

builder.RegisterServiceableBusHandler<TestEvent, TestEventServiceBusHandler>();

builder.AddServiceableBusEventSender<TestEvent>(TestEvent.Topic);

builder.AddServiceableBus();

var app = builder.Build();

//Test fire the sender by getting the IServiceableBusPublisher and calling PublishAsync.
var sender = app.Services.GetService<IServiceableBusPublisher>();

await sender.PublishAsync(new TestEvent 
{ 
    MessageTypeName = "TestEvent",
    Payload = new TestEvent.TestEventPayload("Test", 1, 4) 
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
```


## ServiceablePropertyBag

The `ServiceablePropertyBag` class is used to store and manage a collection of key-value pairs. It provides a convenient way to pass additional properties along with your events.

### Example Usage

1. Define a `ServiceablePropertyBag` with properties:
```
var propertyBag = new ServiceablePropertyBag { Properties =[("Property1", "Value1"), ("Property2", 123), ("Property3", true)]};
```
2. Use the `ServiceablePropertyBag` when publishing an event:
```
await sender.PublishAsync(new TestEvent 
{ 
    MessageTypeName = "TestEvent", 
    Payload = new TestEvent.TestEventPayload("Test", 1, 4) 
},
() => propertyBag);
```

## Additional Information

- Ensure that your Azure Service Bus connection string and other settings are correctly configured in the `appsettings.json` file.
- You can add multiple listeners and handlers as needed by following the pattern shown above.

## License

This project is licensed under the MIT License.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## Contact

For any questions or issues, please open an issue on the GitHub repository.
