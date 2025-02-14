namespace ServiceableBus.Contracts;

public class ServiceablePropertyBag
{
    public required (string, object)[] _properties { get; init; }

    public Dictionary<string, object> ToDictionary()
    {
        return _properties.ToDictionary();
    }
}
