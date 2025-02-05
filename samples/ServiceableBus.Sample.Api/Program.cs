using ServiceableBus;
using ServiceableBus.Sample.Api;
using System.Text.Json;
using System.Text.Json.Serialization;
using static ServiceableBus.Sample.Api.TestEvent;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var serviceBusConfig = builder.Configuration.GetSection("ServiceableBus");
var connectionString = serviceBusConfig["ConnectionString"] ?? throw new ArgumentNullException("ServiceableBus:ConnectionString");

builder.AddServiceableBusTopicListener<TestEvent>(TestEvent.Topic);

builder.RegisterServiceableBusHandler<TestEvent, TestEventServiceBusHandler>();

builder.AddServiceableBus(connectionString); // Call the extension method

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var options = new JsonSerializerOptions()
{
    PropertyNameCaseInsensitive = true,
    IncludeFields = true,
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};

options.Converters.Add(new JsonStringEnumConverter());

var ev = new TestEvent {
    Payload = new TestEventPayload("test1", 3, 5) 
};

var meh = JsonSerializer.Serialize(ev, typeof(ServiceableBusEvent<TestEventPayload>), options);

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
