using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Query.Models.Table;

namespace PUSpecSheet.Api.DAL.Query.Mappers;

internal static class NoteMapper
{
    public static SlimPUSpecSheetTableNote MapToSlimNote(this SheetTableNote note)
    {
        return new SlimPUSpecSheetTableNote
        {
            Id = note.Id,
            Note = note.Note,
            SourceSystem = note.SourceSystem,
            IsObsolete = note.IsObsolete,
            CreatedBy = note.CreatedByUser!.DisplayName,
            CreatedDateTime = note.CreatedDateTime
        };
    }

    public static IEnumerable<SlimPUSpecSheetTableNote> MapToSlimNotes(this ICollection<SheetTableNote> notes)
    {
        return notes.Select(MapToSlimNote);
    }
}
