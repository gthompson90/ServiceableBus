namespace ServiceableBus.Contracts;

public class ServiceablePropertyBag
{
    public required (string Key, object Value)[] Properties { get; init; }

    public Dictionary<string, object> ToDictionary()
    {
        return Properties.ToDictionary();
    }
}
