using System.Text.Json.Serialization;

namespace ServiceableBus.Azure.Abstractions;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "messageTypeName")]
public interface IServiceableBusPayload
{
}