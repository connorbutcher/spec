namespace PUSpecSheet.Migrator;

/// <summary>A legacy sheet that can't be migrated as it is, with what is wrong with it.</summary>
public sealed class MigrationException : Exception
{
    public MigrationException()
    {
    }

    public MigrationException(string message)
        : base(message)
    {
    }

    public MigrationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
