using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ServiceableBus.Contracts;

namespace ServiceableBus.Azure;

public abstract class ServiceableBusEvent<T> : IServiceableBusEvent where T : IServiceableBusPayload
{
    [JsonProperty("messageTypeName")]
    public required string MessageTypeName { get; set; }

    [JsonProperty("createdAt")]
    public required DateTime CreatedAt { get; set; }

    [JsonProperty("payload")]
    public required T Payload { get; set; }
}