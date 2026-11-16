namespace PUSpecSheet.Api.Published;

/// <summary>
/// The identifier values a caller narrows published sheets by: every query string parameter other than the
/// ones the action names itself, such as <c>?phase=V6&amp;sheetType=4&amp;serialNumber=ESN-1</c>.
/// </summary>
public static class PublishedIdentifierQuery
{
    public static IReadOnlyDictionary<string, string> From(IQueryCollection query, params string[] reserved)
    {
        ArgumentNullException.ThrowIfNull(query);
        return query
            .Where(parameter => !reserved.Contains(parameter.Key, StringComparer.OrdinalIgnoreCase))
            .ToDictionary(parameter => parameter.Key, parameter => parameter.Value.ToString(), StringComparer.OrdinalIgnoreCase);
    }
}
