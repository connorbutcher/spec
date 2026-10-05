using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;

namespace PUSpecSheet.Api.Tests.Compression;

/// <summary>Controller responses are compressed for clients that ask, and the caching headers stay correct.</summary>
public sealed class ResponseCompressionTests : IAsyncLifetime
{
    private Microsoft.AspNetCore.Builder.WebApplication app = null!;
    private HttpClient client = null!;

    public async Task InitializeAsync()
    {
        (app, client) = await CompressionHost.StartAsync();
    }

    public async Task DisposeAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    [Fact]
    public async Task AskingForGzip_GetsGzip_ThatMatchesTheUncompressedBodyAndIsSmaller()
    {
        var plain = await Get("/test/rows", encoding: null);
        var gzip = await Get("/test/rows", "gzip");

        Assert.Equal("gzip", Assert.Single(gzip.Content.Headers.ContentEncoding));
        var compressed = await gzip.Content.ReadAsByteArrayAsync();
        var original = await plain.Content.ReadAsByteArrayAsync();
        Assert.True(compressed.Length < original.Length / 2, $"{compressed.Length} bytes against {original.Length}");
        Assert.Equal(original, Decompress(compressed, gzip: true));
    }

    [Fact]
    public async Task AskingForBrotli_GetsBrotli()
    {
        var response = await Get("/test/rows", "br");

        Assert.Equal("br", Assert.Single(response.Content.Headers.ContentEncoding));
    }

    [Fact]
    public async Task AcceptingBothAtTheSameWeight_PrefersBrotli()
    {
        var response = await Get("/test/rows", "gzip, br");

        Assert.Equal("br", Assert.Single(response.Content.Headers.ContentEncoding));
    }

    [Fact]
    public async Task AcceptingOnlyGzip_FallsBackToGzip()
    {
        var response = await Get("/test/rows", "gzip, deflate");

        Assert.Equal("gzip", Assert.Single(response.Content.Headers.ContentEncoding));
    }

    [Fact]
    public async Task WithoutAnAcceptEncoding_TheBodyIsSentAsItIs()
    {
        var response = await Get("/test/rows", encoding: null);

        Assert.Empty(response.Content.Headers.ContentEncoding);
    }

    [Theory]
    [InlineData("gzip")]
    [InlineData("br")]
    [InlineData(null)]
    public async Task CompressibleResponses_SayTheyVaryByAcceptEncoding(string? encoding)
    {
        var response = await Get("/test/rows", encoding);

        Assert.Contains("Accept-Encoding", response.Headers.Vary);
    }

    [Fact]
    public async Task ProblemDetailsAreCompressedToo()
    {
        var response = await Get("/test/problem", "gzip");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("gzip", Assert.Single(response.Content.Headers.ContentEncoding));
    }

    [Fact]
    public async Task ContentThatIsAlreadyCompressed_IsLeftAlone()
    {
        var response = await Get("/test/image", "gzip");

        Assert.Empty(response.Content.Headers.ContentEncoding);
    }

    [Fact]
    public async Task ACompressedAnswerCarriesAWeakETag_AndAnUncompressedOneStaysStrong()
    {
        var gzip = await Get("/test/published", "gzip");
        var plain = await Get("/test/published", encoding: null);

        Assert.True(gzip.Headers.ETag?.IsWeak);
        Assert.Equal(CompressionTestController.Tag.Tag.Value, gzip.Headers.ETag?.Tag);
        Assert.False(plain.Headers.ETag?.IsWeak);
    }

    [Theory]
    [InlineData("gzip")]
    [InlineData("br")]
    [InlineData(null)]
    public async Task SendingTheETagBack_Gets304WithNoBody_WhicheverRepresentationItCameFrom(string? encoding)
    {
        var first = await Get("/test/published", encoding);
        var etag = first.Headers.ETag!;

        var again = await Get("/test/published", encoding, ifNoneMatch: etag);

        Assert.Equal(HttpStatusCode.NotModified, again.StatusCode);
        Assert.Empty(await again.Content.ReadAsByteArrayAsync());
        Assert.Empty(again.Content.Headers.ContentEncoding);
        Assert.Equal(CompressionTestController.Tag.Tag.Value, again.Headers.ETag?.Tag);
    }

    [Fact]
    public async Task AnETagFromOneRepresentation_StillMatchesWhenTheClientAsksForAnother()
    {
        // A weak tag from a gzip answer is still a valid If-None-Match for the same version without gzip.
        var gzip = await Get("/test/published", "gzip");

        var plain = await Get("/test/published", encoding: null, ifNoneMatch: gzip.Headers.ETag);

        Assert.Equal(HttpStatusCode.NotModified, plain.StatusCode);
    }

    private async Task<HttpResponseMessage> Get(string path, string? encoding, EntityTagHeaderValue? ifNoneMatch = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.AcceptEncoding.Clear();
        if (encoding is not null)
        {
            foreach (var part in encoding.Split(',', StringSplitOptions.TrimEntries))
            {
                request.Headers.AcceptEncoding.Add(StringWithQualityHeaderValue.Parse(part));
            }
        }

        if (ifNoneMatch is not null)
        {
            request.Headers.IfNoneMatch.Add(ifNoneMatch);
        }

        var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        await response.Content.LoadIntoBufferAsync();
        return response;
    }

    private static byte[] Decompress(byte[] data, bool gzip)
    {
        using var input = new MemoryStream(data);
        using Stream decoder = gzip ? new GZipStream(input, CompressionMode.Decompress) : new BrotliStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        decoder.CopyTo(output);
        return output.ToArray();
    }
}
