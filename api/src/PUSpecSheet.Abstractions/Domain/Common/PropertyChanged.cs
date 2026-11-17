namespace PUSpecSheet.Abstractions.Domain.Common;

public sealed record PropertyChanged<T>
{
    public T? Value { get; init; }

    public required bool HasChanged { get; init; }
}
