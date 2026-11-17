using System.Security.Cryptography;
using System.Text;
using PUSpecSheet.DataAccess.Published.Versions;

namespace PUSpecSheet.DataAccess.Published.Lookups;

public sealed class PublishedLookupResolution
{
    public PublishedLookupResolution(
        PublishedLookupCriteria criteria,
        IReadOnlyList<PublishedLookupHit> hits,
        IReadOnlyDictionary<int, ResolvedSheetVersion> versions)
    {
        Criteria = criteria;
        Hits = hits;
        Versions = versions;
        Key = BuildKey();
    }

    public PublishedLookupCriteria Criteria { get; }

    public IReadOnlyList<PublishedLookupHit> Hits { get; }

    public IReadOnlyDictionary<int, ResolvedSheetVersion> Versions { get; }

    // A published version never changes, so an equal key means an equal answer.
    public string Key { get; }

    private string BuildKey()
    {
        var text = new StringBuilder()
            .Append(Criteria.Key).Append('\n')
            .Append(Criteria.Value).Append('\n')
            .Append(Criteria.PhaseCode).Append('\n')
            .Append(Criteria.SheetTypeId).Append('\n');

        foreach (var version in Versions.Values.OrderBy(version => version.SheetPublicId))
        {
            text.Append(version.SheetPublicId.ToString("N")).Append('v').Append(version.VersionNumber).Append(';');
        }

        foreach (var cellId in Hits.Select(hit => hit.CellId).Order())
        {
            text.Append('c').Append(cellId).Append(';');
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text.ToString()));
        return "lookup-" + Convert.ToHexStringLower(hash.AsSpan(0, 12));
    }
}
