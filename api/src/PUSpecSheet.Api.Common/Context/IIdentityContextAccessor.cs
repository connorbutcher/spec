namespace PUSpecSheet.Api.Common.Context;

/// <summary>
/// The identity context accessor.
/// </summary>
public interface IIdentityContextAccessor
{
    /// <summary>
    /// The Identity object GUID.
    /// </summary>
    Guid ObjectGuid { get; }

    /// <summary>
    /// The name of the identity.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The email of the identity.
    /// </summary>
    string Email { get; }
}
