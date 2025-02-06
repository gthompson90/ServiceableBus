using System.Text.Json.Serialization;

namespace ServiceableBus;

[JsonPolymorphic(TypeDiscriminatorPropertyName="messageTypeName")]
public interface IServiceableBusPayload
{
}