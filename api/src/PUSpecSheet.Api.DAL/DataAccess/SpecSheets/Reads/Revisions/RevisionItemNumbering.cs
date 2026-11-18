namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads;

/// <summary>
/// Ids for what the revisions have no items for (workflow step actors), numbered from 1 within
/// the sheet for each kind.
/// </summary>
internal class RevisionItemNumbering
{
    private readonly Dictionary<string, int> _last = [];

    public int Next(string kind)
    {
        var next = _last.GetValueOrDefault(kind) + 1;
        _last[kind] = next;
        return next;
    }
}
