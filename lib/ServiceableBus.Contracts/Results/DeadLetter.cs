namespace ServiceableBus.Contracts.Results;

public record DeadLetter(Exception? ex) : IServiceableResult;