namespace PUSpecSheet.Application.Common;

/// <summary>The request is well-formed but its values don't make sense together. Maps to 400.</summary>
public sealed class InvalidRequestException : Exception
{
    public InvalidRequestException(string message)
        : base(message)
    {
    }
}
