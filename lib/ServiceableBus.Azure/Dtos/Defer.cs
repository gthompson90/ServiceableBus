using ServiceableBus.Contracts;

namespace ServiceableBus.Azure.Dtos;

public record Defer(int deferDurationInSeconds) : IServiceableResult;