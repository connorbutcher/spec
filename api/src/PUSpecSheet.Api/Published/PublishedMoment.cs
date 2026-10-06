using System.Globalization;
using PUSpecSheet.Application.Common;

namespace PUSpecSheet.Api.Published;

/// <summary>Reads the date and time a caller gives to ask what was published at a moment.</summary>
public static class PublishedMoment
{
    /// <summary>The moment in UTC. Text without an offset is taken as UTC.</summary>
    /// <exception cref="InvalidRequestException">The text is not a date and time.</exception>
    public static DateTime ParseUtc(string text)
    {
        if (!DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        {
            throw new InvalidRequestException($"\"{text}\" is not a date and time.");
        }

        return parsed.UtcDateTime;
    }
}
