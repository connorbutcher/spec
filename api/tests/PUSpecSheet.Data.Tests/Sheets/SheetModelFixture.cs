using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace PUSpecSheet.Data.Tests.Sheets;

/// <summary>The EF model of the database, built without connecting to anything.</summary>
internal static class SheetModelFixture
{
    public static IModel Model { get; } = Build();

    private static IModel Build()
    {
        var options = new DbContextOptionsBuilder<PuSpecSheetDbContext>()
            .UseSqlServer("Server=unused;Database=unused")
            .Options;
        using var db = new PuSpecSheetDbContext(options);
        return db.Model;
    }
}
