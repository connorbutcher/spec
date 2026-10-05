namespace PUSpecSheet.Api.Tests.Compression;

/// <summary>The "Compression" settings turn it off, and it is on without any settings.</summary>
public sealed class CompressionSettingsTests
{
    [Fact]
    public async Task WhenSwitchedOff_NothingIsCompressed()
    {
        var (app, client) = await CompressionHost.StartAsync(new Dictionary<string, string?> { ["Compression:Enabled"] = "false" });
        await using (app)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/test/rows");
            request.Headers.AcceptEncoding.ParseAdd("gzip, br");

            using var response = await client.SendAsync(request);

            Assert.Empty(response.Content.Headers.ContentEncoding);
        }
    }

    [Fact]
    public async Task WithNoSettingsAtAll_ItIsOn()
    {
        var (app, client) = await CompressionHost.StartAsync();
        await using (app)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/test/rows");
            request.Headers.AcceptEncoding.ParseAdd("gzip");

            using var response = await client.SendAsync(request);

            Assert.Equal("gzip", Assert.Single(response.Content.Headers.ContentEncoding));
        }
    }
}
