using Microsoft.Net.Http.Headers;
using PUSpecSheet.Application.Published;

namespace PUSpecSheet.Api.Published;

/// <summary>
/// HTTP caching for published sheet reads. The ETag names one version read with one selection, and a
/// published version never changes, so a matching ETag always means the caller's copy is still right.
/// </summary>
public static class PublishedSheetHttpCache
{
    /// <summary>For a request that names a fixed version or a past moment: the answer can be kept for good.</summary>
    private const string Fixed = "private, max-age=31536000, immutable";

    /// <summary>For "latest": keep the answer, but check it is still the newest before using it.</summary>
    private const string Revalidate = "private, no-cache";

    public static EntityTagHeaderValue ETagFor(ResolvedSheetVersion version, PublishedSheetSelection selection)
    {
        return new EntityTagHeaderValue($"\"{version.KeyFor(selection)}\"");
    }

    public static void Apply(HttpResponse response, EntityTagHeaderValue etag, bool isFixed)
    {
        var headers = response.GetTypedHeaders();
        headers.ETag = etag;
        response.Headers.CacheControl = isFixed ? Fixed : Revalidate;
    }

    /// <summary>Whether the caller already holds this answer, from the ETag it sent in <c>If-None-Match</c>.</summary>
    public static bool CallerHas(HttpRequest request, EntityTagHeaderValue etag)
    {
        var sent = request.GetTypedHeaders().IfNoneMatch;
        return sent.Any(candidate => candidate.Equals(EntityTagHeaderValue.Any) || candidate.Compare(etag, useStrongComparison: false));
    }
}
