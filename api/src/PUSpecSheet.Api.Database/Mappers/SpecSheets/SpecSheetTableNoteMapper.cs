using PUSpecSheet.Abstractions.Domain.SpecSheets;
using PUSpecSheet.Api.Database.Mappers.Access;
using PUSpecSheet.Api.Database.Schema.SpecSheets;

namespace PUSpecSheet.Api.Database.Mappers.SpecSheets;

public static class SpecSheetTableNoteMapper
{
    public static SpecSheetTableNote MapToDomain(this DbSpecSheetTableNote dbSpecSheetTableNote)
    {
        return new SpecSheetTableNote
        {
            Id = dbSpecSheetTableNote.Id,
            SpecSheetTableId = dbSpecSheetTableNote.SpecSheetTableId,
            Note = dbSpecSheetTableNote.Note,
            SourceSystem = dbSpecSheetTableNote.SourceSystem,
            IsObsolete = dbSpecSheetTableNote.IsObsolete,
            CreatedByUserId = dbSpecSheetTableNote.CreatedByUserId,
            CreatedDateTime = dbSpecSheetTableNote.CreatedDateTime,
            CreatedByUser = dbSpecSheetTableNote.CreatedByUser!.MapToDomain()
        };
    }

    public static IEnumerable<SpecSheetTableNote> MapToDomain(this ICollection<DbSpecSheetTableNote> dbSpecSheetTableNotes)
    {
        return dbSpecSheetTableNotes.Select(MapToDomain);
    }

    public static IQueryable<SpecSheetTableNote> MapToDomain(this IQueryable<DbSpecSheetTableNote> dbSpecSheetTableNotes)
    {
        return dbSpecSheetTableNotes
            .Select(n => n.MapToDomain());
    }
}
