using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Domain.Phases;

namespace PUSpecSheet.Data.Seeding;

/// <summary>
/// Adds sample phases to an empty development database so the UI has something to show until phases can
/// be managed in the admin section. Does nothing once any phase exists.
/// </summary>
public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(PuSpecSheetDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Phases.AnyAsync(cancellationToken))
        {
            return;
        }

        var v6 = CreatePhase("V6", "V6 build", 1, [1, 2, 3, 4, 5, 6]);
        v6.ChildPhases.Add(CreatePhase("01-A2", null, 1, [1, 2, 3]));
        v6.ChildPhases.Add(CreatePhase("A3", null, 2, [1, 2, 6]));
        v6.ChildPhases.Add(CreatePhase("A4", null, 3, [1, 4]));
        v6.ChildPhases.Add(CreatePhase("A5", null, 4, [1, 2, 5, 6]));

        var sc = CreatePhase("SC", null, 2, [1, 5]);

        db.Phases.AddRange(v6, sc);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static Phase CreatePhase(string code, string? description, int displayOrder, int[] sheetTypeIds)
    {
        var phase = new Phase
        {
            Code = code,
            Description = description,
            DisplayOrder = displayOrder,
        };

        foreach (var sheetTypeId in sheetTypeIds)
        {
            phase.SheetTypes.Add(new PhaseSheetType { SheetTypeId = sheetTypeId });
        }

        return phase;
    }
}
