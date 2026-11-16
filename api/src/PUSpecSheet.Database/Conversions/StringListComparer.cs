using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace PUSpecSheet.Database.Conversions;

/// <summary>Compares lists of strings by their items, so a list changed in place is saved.</summary>
internal sealed class StringListComparer
    : ValueComparer<List<string>?>
{
    public StringListComparer()
        : base(
            (left, right) => left == null ? right == null : right != null && left.SequenceEqual(right),
            list => list == null ? 0 : list.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode(StringComparison.Ordinal))),
            list => list == null ? null : list.ToList())
    {
    }
}
