namespace PUSpecSheet.Application.Common;

/// <summary>The user may not do this to this particular record, whatever their permissions. Maps to 403.</summary>
public sealed class ForbiddenException(string message) : Exception(message);
