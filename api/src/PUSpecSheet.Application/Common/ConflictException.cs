namespace PUSpecSheet.Application.Common;

/// <summary>The change clashes with existing data, such as a duplicate name or a record still in use. Maps to 409.</summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
