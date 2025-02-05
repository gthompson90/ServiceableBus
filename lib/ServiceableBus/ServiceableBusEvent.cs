using System.Text.Json.Serialization;

namespace ServiceableBus;

public abstract class ServiceableBusEvent<T> : IServiceableBusEvent where T : IServiceableBusPayload
{
    [JsonPropertyName("messageTypeName")]
    public string MessageTypeName { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("payload")]
    public T Payload { get; set; }
}