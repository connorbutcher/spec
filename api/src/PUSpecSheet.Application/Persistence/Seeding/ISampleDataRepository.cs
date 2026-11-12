namespace PUSpecSheet.Application.Persistence.Seeding;

/// <summary>Clears a development database down to its reference data so the showcase can be built on it.</summary>
public interface ISampleDataRepository
{
    /// <summary>
    /// Deletes every sheet with all its versions, drafts and values, every table template, and every cell
    /// type that isn't one of the built-in ones the migrations add. Phases, sheet types, users and permissions
    /// are kept. It runs in one transaction, so a failure leaves the database as it was.
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken);
}
