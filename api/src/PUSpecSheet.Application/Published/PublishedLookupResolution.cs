using System.Security.Cryptography;
using System.Text;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// What a lookup found before any data is read: the cells that hold the value, and the version of each
/// sheet they are read at. <see cref="Key"/> names the answer, so a caller that already holds it can be
/// told so without reading anything more.
/// </summary>
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

    /// <summary>The cells found, in the order their matches are returned.</summary>
    public IReadOnlyList<PublishedLookupHit> Hits { get; }

    /// <summary>The version each sheet is read at, by the sheet's database id.</summary>
    public IReadOnlyDictionary<int, ResolvedSheetVersion> Versions { get; }

    /// <summary>
    /// The same for the same question answered from the same cells at the same sheet versions. A published
    /// version never changes, so an equal key means an equal answer.
    /// </summary>
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
