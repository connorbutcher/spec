using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;
using PUSpecSheet.Query.Models.Table;

namespace PUSpecSheet.Api.DAL.Query.Mappers;

internal static class NoteMapper
{
    public static SlimPUSpecSheetTableNote MapToSlimNote(this SpecSheetTableNote note)
    {
        return new SlimPUSpecSheetTableNote
        {
            Id = note.Id,
            Note = note.Note,
            SourceSystem = note.SourceSystem,
            IsObsolete = note.IsObsolete,
            CreatedBy = note.CreatedByUser.DisplayName,
            CreatedDateTime = note.CreatedDateTime
        };
    }

    public static IEnumerable<SlimPUSpecSheetTableNote> MapToSlimNotes(this ICollection<SpecSheetTableNote> notes)
    {
        return notes.Select(MapToSlimNote);
    }
}
