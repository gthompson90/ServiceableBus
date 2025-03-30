using ServiceableBus.Contracts;

namespace ServiceableBus.Azure.Dtos;

public record DeadLetter(Exception? ex = null) : IServiceableResult;