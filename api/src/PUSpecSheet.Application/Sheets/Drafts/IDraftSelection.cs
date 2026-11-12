using PUSpecSheet.Database.Entities.Sheets;

namespace PUSpecSheet.Application.Sheets.Drafts;

/// <summary>Which of a sheet's drafts a publish takes, written once for every kind of revision.</summary>
public interface IDraftSelection
{
    IQueryable<TRevision> Apply<TRevision>(IQueryable<TRevision> revisions)
        where TRevision : class, ISheetRevision;
}
