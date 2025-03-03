namespace ServiceableBus.Azure.Abstractions;

public interface IServiceableRetryOptions
{
    public int MaxRetries { get; init; }

    public int DelaySeconds { get; init; }
}