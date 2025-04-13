namespace ServiceableBus.Contracts.Results;

public class ServiceableResult
{
    public static Complete Complete() => new();

    public static Defer Defer(int delayInMinutes) => new Defer(delayInMinutes);

    public static Abandon Abandon() => new Abandon();

    public static DeadLetter DeadLetter(Exception? ex = null) => new DeadLetter(ex);
}
