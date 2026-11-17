namespace PUSpecSheet.DataAccess.Common;

public sealed class InvalidRequestException : Exception
{
    public InvalidRequestException(string message)
        : base(message)
    {
    }
}
