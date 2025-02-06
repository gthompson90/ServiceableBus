using ServiceableBus.Extensions;
using ServiceableBus.Sample.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

//Adding options is required for the ServiceableBus configuration.
builder.Services.AddOptions();

//Add your Listeners and Handlers here BEFORE adding the ServiceableBus.
builder.AddServiceableBusTopicListener<TestEvent>(TestEvent.Topic);
builder.RegisterServiceableBusHandler<TestEvent, TestEventServiceBusHandler>();
builder.AddServiceableBus();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();