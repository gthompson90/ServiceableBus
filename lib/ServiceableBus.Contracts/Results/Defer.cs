namespace ServiceableBus.Contracts.Results;

public record Defer(int durationInMinutes) : IServiceableResult;