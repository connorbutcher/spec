using System.Globalization;

namespace PUSpecSheet.Api.Collaboration;

/// <summary>The SignalR groups a connection joins when it opens a sheet.</summary>
public static class SheetGroups
{
    /// <summary>Everyone who has the sheet open.</summary>
    public static string Sheet(int sheetId)
    {
        return string.Create(CultureInfo.InvariantCulture, $"sheet:{sheetId}");
    }

    /// <summary>One person's connections to the sheet, however many tabs they have it open in.</summary>
    public static string User(int sheetId, int userId)
    {
        return string.Create(CultureInfo.InvariantCulture, $"sheet:{sheetId}:user:{userId}");
    }
}
