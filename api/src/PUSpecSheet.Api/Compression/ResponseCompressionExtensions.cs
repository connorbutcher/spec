using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Net.Http.Headers;

namespace PUSpecSheet.Api.Compression;

public static class ResponseCompressionExtensions
{
    /// <summary>
    /// Registers response compression for every controller, problem-details and API documentation response:
    /// Brotli for clients that accept it, gzip for the rest. Types that are already compressed (images, fonts,
    /// archives) aren't in the list, so they are sent as they are. Settings come from the
    /// <see cref="CompressionSettings.SectionName"/> configuration section.
    /// </summary>
    public static IServiceCollection AddPuSpecSheetResponseCompression(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(CompressionSettings.SectionName).Get<CompressionSettings>() ?? new CompressionSettings();
        services.AddSingleton(settings);

        if (!settings.Enabled)
        {
            return services;
        }

        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = settings.EnableForHttps;

            // Brotli first: when a client accepts both at the same weight it is preferred, with gzip as the fallback.
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();

            // JSON of every flavour (problem details are application/problem+json, the OpenAPI document and
            // the Swagger UI scripts and styles are text), nothing binary.
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
            [
                "application/*+json",
                "application/problem+json",
                "application/problem+xml",
                "image/svg+xml",
            ]);
        });

        services.Configure<BrotliCompressionProviderOptions>(options => options.Level = settings.Level);
        services.Configure<GzipCompressionProviderOptions>(options => options.Level = settings.Level);
        return services;
    }

    /// <summary>
    /// Compresses responses written after this point, so it goes first: problem details from the exception
    /// handler are compressed as well. A compressed answer carries a weak ETag, because its bytes are not the
    /// same as the uncompressed ones, and a client that sends that tag back still gets a 304.
    /// </summary>
    public static IApplicationBuilder UsePuSpecSheetResponseCompression(this IApplicationBuilder app)
    {
        var settings = app.ApplicationServices.GetRequiredService<CompressionSettings>();
        if (!settings.Enabled)
        {
            return app;
        }

        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                AnnounceVariation(context);
                WeakenETagWhenCompressed(context.Response);
                return Task.CompletedTask;
            });
            await next();
        });
        return app.UseResponseCompression();
    }

    /// <summary>
    /// Says the answer depends on Accept-Encoding even when this client sent none, so a cache never hands the
    /// uncompressed copy to a client that asked for gzip as if it were the only version. The compression
    /// middleware only adds it when it actually compresses.
    /// </summary>
    private static void AnnounceVariation(HttpContext context)
    {
        var provider = context.RequestServices.GetRequiredService<IResponseCompressionProvider>();
        if (provider.ShouldCompressResponse(context) && !context.Response.Headers.Vary.Any(value => value is not null && value.Contains(HeaderNames.AcceptEncoding, StringComparison.OrdinalIgnoreCase)))
        {
            context.Response.Headers.Append(HeaderNames.Vary, HeaderNames.AcceptEncoding);
        }
    }

    /// <summary>
    /// A strong ETag promises the exact same bytes, which a gzip or Brotli body isn't. Marking it weak ("W/")
    /// keeps it a valid validator for the compressed representation; <c>If-None-Match</c> compares weakly, so
    /// the client's next request still matches.
    /// </summary>
    private static void WeakenETagWhenCompressed(HttpResponse response)
    {
        if (string.IsNullOrEmpty(response.Headers.ContentEncoding.ToString()))
        {
            return;
        }

        var etag = response.GetTypedHeaders().ETag;
        if (etag is { IsWeak: false })
        {
            response.GetTypedHeaders().ETag = new EntityTagHeaderValue(etag.Tag, isWeak: true);
        }
    }
}
