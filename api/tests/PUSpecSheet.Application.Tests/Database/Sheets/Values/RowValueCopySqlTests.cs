using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Sheets.Queries;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Tests.Database.Sheets.Values;

/// <summary>
/// The batch that copies a row revision's values into a new draft is built from the EF model, so these
/// check it against the model without a database: every value table is copied, by revision, and the
/// computed lookup column is left to the database.
/// </summary>
public sealed class RowValueCopySqlTests : IDisposable
{
    private readonly PuSpecSheetDbContext db = new(
        new DbContextOptionsBuilder<PuSpecSheetDbContext>()
            .UseSqlServer("Server=unused;Database=unused")
            .Options);

    public void Dispose()
    {
        db.Dispose();
    }

    [Fact]
    public void CopiesEveryValueTable_FromOneRevisionToAnother()
    {
        var sql = RowValueCopySql.Build(db.Model);

        Assert.Contains(
            "INSERT INTO [cell].[TextValues] ([SheetRowRevisionId], [SheetCellId], [Value]) SELECT {1}, [SheetCellId], [Value] FROM [cell].[TextValues] WHERE [SheetRowRevisionId] = {0};",
            sql);
        Assert.Contains("INSERT INTO [cell].[NumericValues] ([SheetRowRevisionId], [SheetCellId], [Value])", sql);
        Assert.Contains("INSERT INTO [cell].[DateValues] ([SheetRowRevisionId], [SheetCellId], [Value])", sql);
        Assert.Contains("INSERT INTO [cell].[BooleanValues] ([SheetRowRevisionId], [SheetCellId], [Value])", sql);
        Assert.Contains("INSERT INTO [cell].[OptionValues] ([SheetRowRevisionId], [SheetCellId], [CellTypeOptionId])", sql);
        Assert.Contains("INSERT INTO [cell].[CellSettings] ([SheetRowRevisionId], [SheetCellId], [Settings])", sql);
        Assert.Contains("INSERT INTO [cell].[LabelledTextValues] ([SheetRowRevisionId], [SheetCellId], [SlotKey], [Position], [Value])", sql);
        Assert.Equal(7, sql.Split("INSERT INTO").Length - 1);
    }

    [Fact]
    public void LeavesTheComputedLookupColumnToTheDatabase()
    {
        Assert.DoesNotContain("LookupValue", RowValueCopySql.Build(db.Model));
    }
}
