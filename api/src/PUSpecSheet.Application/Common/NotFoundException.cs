namespace PUSpecSheet.Application.Common;

/// <summary>The requested record doesn't exist. Maps to 404.</summary>
public sealed class NotFoundException(string message) : Exception(message);
