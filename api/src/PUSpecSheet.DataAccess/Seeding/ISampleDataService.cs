namespace PUSpecSheet.DataAccess.Seeding;

public interface ISampleDataService
{
    Task ClearAsync(CancellationToken cancellationToken);
}
