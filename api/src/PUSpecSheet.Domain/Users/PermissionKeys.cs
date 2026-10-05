namespace PUSpecSheet.Domain.Users;

/// <summary>
/// The key of every permission. Each one is also a row in the Permissions table and the name of the
/// API's authorization policy for it. To add one, add its key here, to <see cref="All"/> and to the
/// Permissions seed data, then create a migration.
/// </summary>
public static class PermissionKeys
{
    /// <summary>Add phases and choose which sheet types each phase has.</summary>
    public const string PhasesManage = "phases.manage";

    /// <summary>Add sheet types.</summary>
    public const string SheetTypesManage = "sheetTypes.manage";

    public static IReadOnlyList<string> All { get; } = [PhasesManage, SheetTypesManage];
}
