using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data.Repositories.Sheets.Values;

namespace PUSpecSheet.Data.Tests.Sheets.Values;

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
            "INSERT INTO [values].[TextValues] ([SheetRowRevisionId], [SheetCellId], [Value]) SELECT {1}, [SheetCellId], [Value] FROM [values].[TextValues] WHERE [SheetRowRevisionId] = {0};",
            sql);
        Assert.Contains("INSERT INTO [values].[NumericValues] ([SheetRowRevisionId], [SheetCellId], [Value])", sql);
        Assert.Contains("INSERT INTO [values].[DateValues] ([SheetRowRevisionId], [SheetCellId], [Value])", sql);
        Assert.Contains("INSERT INTO [values].[BooleanValues] ([SheetRowRevisionId], [SheetCellId], [Value])", sql);
        Assert.Contains("INSERT INTO [values].[OptionValues] ([SheetRowRevisionId], [SheetCellId], [CellTypeOptionId])", sql);
        Assert.Contains("INSERT INTO [values].[CellSettings] ([SheetRowRevisionId], [SheetCellId], [Settings])", sql);
        Assert.Contains("INSERT INTO [values].[LabelledTextValues] ([SheetRowRevisionId], [SheetCellId], [SlotKey], [Position], [Value])", sql);
        Assert.Equal(7, sql.Split("INSERT INTO").Length - 1);
    }

    [Fact]
    public void LeavesTheComputedLookupColumnToTheDatabase()
    {
        Assert.DoesNotContain("LookupValue", RowValueCopySql.Build(db.Model));
    }
}
