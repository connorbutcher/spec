namespace PUSpecSheet.Api.Common.Claims;

/// <summary>
/// Provides access to identity claims.
/// </summary>
public interface IClaimsAccessor
{
    /// <summary>
    /// Gets the object ID of the request identity.
    /// </summary>
    /// <returns>The decoded GUID of the oid claim.</returns>
    Guid GetObjectId();

    /// <summary>
    /// Gets the name of the request identity.
    /// </summary>
    /// <returns>The value of the name claim.</returns>
    string GetName();

    /// <summary>
    /// Tries to get the name of the request identity.
    /// </summary>
    /// <returns>The value of the name claim, or Null if none found.</returns>
    string? TryGetName();

    /// <summary>
    /// Gets the email of the request identity.
    /// </summary>
    /// <returns>The value of the email claim.</returns>
    string GetEmail();
}
