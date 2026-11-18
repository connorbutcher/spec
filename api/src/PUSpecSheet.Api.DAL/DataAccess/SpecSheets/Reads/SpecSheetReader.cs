using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads.Revisions;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads;

internal class SpecSheetReader : ISpecSheetReader
{
    private readonly SpecSheetCopyReader _copyReader;
    private readonly SpecSheetRevisionReader _revisionReader;
    private readonly SheetEditSwitch _editSwitch;
    private readonly IOptionsMonitor<SheetReadOptions> _options;
    private readonly ILogger<ISpecSheetReader> _logger;

    public SpecSheetReader(SpecSheetCopyReader copyReader,
        SpecSheetRevisionReader revisionReader,
        SheetEditSwitch editSwitch,
        IOptionsMonitor<SheetReadOptions> options,
        ILogger<ISpecSheetReader> logger)
    {
        ArgumentNullException.ThrowIfNull(copyReader, nameof(copyReader));
        ArgumentNullException.ThrowIfNull(revisionReader, nameof(revisionReader));
        ArgumentNullException.ThrowIfNull(editSwitch, nameof(editSwitch));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _copyReader = copyReader;
        _revisionReader = revisionReader;
        _editSwitch = editSwitch;
        _options = options;
        _logger = logger;
    }

    public async Task<SpecSheet> ReadAsync(int id, CancellationToken cancellationToken)
    {
        var draftSheetId = await _editSwitch.FindDraftSheetIdAsync(id, cancellationToken);
        if (draftSheetId.HasValue)
        {
            return await _revisionReader.ReadDraftAsync(id, draftSheetId.Value, cancellationToken);
        }

        // A version published on the revisions has nothing in its copy, whatever the read source.
        if (await _editSwitch.IsOnRevisionsAsync(id, cancellationToken))
        {
            var published = await _revisionReader.ReadPublishedAsync(id, cancellationToken);
            if (published is not null)
            {
                return published;
            }
        }

        switch (_options.CurrentValue.Source)
        {
            case SheetReadSource.Revisions:
                var fromRevisions = await _revisionReader.ReadPublishedAsync(id, cancellationToken);
                if (fromRevisions is not null)
                {
                    return fromRevisions;
                }

                return await _copyReader.ReadAsync(id, cancellationToken);

            case SheetReadSource.Compare:
                var fromCopy = await _copyReader.ReadAsync(id, cancellationToken);
                if (fromCopy.IsPublished)
                {
                    await CompareAsync(fromCopy, cancellationToken);
                }

                return fromCopy;

            default:
                return await _copyReader.ReadAsync(id, cancellationToken);
        }
    }

    // A comparison never gets in the way of the read: whatever happens, the copy is what is returned.
    private async Task CompareAsync(SpecSheet fromCopy, CancellationToken cancellationToken)
    {
        try
        {
            var fromRevisions = await _revisionReader.ReadPublishedAsync(fromCopy.Id, cancellationToken);
            if (fromRevisions is null)
            {
                _logger.LogInformation("SpecSheet with Id {SpecSheetId} has no version in the revisions yet; it can't be compared.", fromCopy.Id);
                return;
            }

            var difference = SpecSheetReadComparer.Difference(fromCopy, fromRevisions);
            if (difference is null)
            {
                _logger.LogInformation("SpecSheet with Id {SpecSheetId} reads the same from its copy and from the revisions.", fromCopy.Id);
                return;
            }

            _logger.LogWarning("SpecSheet with Id {SpecSheetId} reads differently from the revisions than from its copy:\n{Difference}", fromCopy.Id, difference);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "SpecSheet with Id {SpecSheetId} couldn't be read from the revisions to compare. Message: {Message}", fromCopy.Id, ex.Message);
        }
    }
}