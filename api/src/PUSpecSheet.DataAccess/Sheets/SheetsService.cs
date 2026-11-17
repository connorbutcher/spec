using PUSpecSheet.Abstractions.Approvals;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Approvals;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Phases;
using PUSpecSheet.DataAccess.Sheets.Catalog;
using PUSpecSheet.DataAccess.Sheets.Drafts;
using PUSpecSheet.DataAccess.Sheets.Editing;
using PUSpecSheet.DataAccess.Sheets.Publishing;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Sheets.Reading;
using PUSpecSheet.DataAccess.Users;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Sheets;

public sealed class SheetsService : ISheetsService
{
    private readonly PuSpecSheetDbContext _db;
    private readonly SheetReader _reader;
    private readonly SheetPublisher _publisher;
    private readonly DraftSweeper _sweeper;
    private readonly SubmissionReader _submissionReader;
    private readonly ICurrentUser _currentUser;
    private readonly ISheetTablesService _tables;
    private readonly ISubmissionsService _submissionsService;
    private readonly SheetCreator _creator;
    private readonly IPhaseDataService _phases;
    private readonly ISubmissionDataService _submissionData;

    public SheetsService(
        PuSpecSheetDbContext db,
        SheetReader reader,
        SheetPublisher publisher,
        DraftSweeper sweeper,
        SubmissionReader submissionReader,
        ICurrentUser currentUser,
        ISheetTablesService tables,
        ISubmissionsService submissionsService,
        SheetCreator creator,
        IPhaseDataService phases,
        ISubmissionDataService submissionData)
    {
        _db = db;
        _reader = reader;
        _publisher = publisher;
        _sweeper = sweeper;
        _submissionReader = submissionReader;
        _currentUser = currentUser;
        _tables = tables;
        _submissionsService = submissionsService;
        _creator = creator;
        _phases = phases;
        _submissionData = submissionData;
    }

    public async Task<Sheet> OpenAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        var sheetId = await SheetQueries.FindIdAsync(_db, phaseId, sheetTypeId, cancellationToken);

        if (sheetId is null)
        {
            sheetId = await _creator.CreateUnidentifiedAsync(phaseId, sheetTypeId, cancellationToken);
        }

        return await GetAsync(sheetId.Value, SheetViewPoint.Live, cancellationToken);
    }

    public async Task<Sheet> GetAsync(int sheetId, SheetViewPoint view, CancellationToken cancellationToken)
    {
        if (view.ReviewedSubmissionId is { } submissionId)
        {
            await _submissionReader.EnsureCanReviewAsync(sheetId, submissionId, _currentUser.UserId, cancellationToken);
        }

        if (view.IsLive)
        {
            await _sweeper.SweepAsync(sheetId, cancellationToken);
        }

        return await _reader.ReadAsync(sheetId, view, cancellationToken);
    }

    public async Task<Sheet> GetForReviewAsync(int submissionId, CancellationToken cancellationToken)
    {
        var sheetId = await _submissionData.FindSheetIdAsync(submissionId, cancellationToken)
            ?? throw new NotFoundException($"Submission {submissionId} was not found.");
        return await GetAsync(sheetId, SheetViewPoint.Reviewing(submissionId), cancellationToken);
    }

    public async Task<IReadOnlyList<SheetSummary>> ListAsync(int phaseId, CancellationToken cancellationToken)
    {
        if (!await _phases.ExistsAsync(phaseId, cancellationToken))
        {
            throw new NotFoundException($"Phase {phaseId} was not found.");
        }

        return await SheetCatalogQueries.ListAsync(_db, phaseId, null, cancellationToken);
    }

    public async Task<SheetSummary> CreateAsync(int phaseId, CreateSheetRequest request, CancellationToken cancellationToken)
    {
        var sheetId = await _creator.CreateAsync(phaseId, request, cancellationToken);
        var created = await SheetCatalogQueries.ListAsync(_db, phaseId, sheetId, cancellationToken);
        return created[0];
    }

    public async Task<SheetRowsChange> GetRowsAsync(int sheetId, IReadOnlyCollection<int> rowIds, CancellationToken cancellationToken)
    {
        var sheet = await _reader.ReadLiveAsync(sheetId, cancellationToken);
        return SheetRowsChanges.From(sheet, rowIds);
    }

    public async Task<Sheet> PublishAsync(int sheetId, string? note, PublishScope scope, CancellationToken cancellationToken)
    {
        await _publisher.PublishAsync(sheetId, note, scope, cancellationToken);
        return await _reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public Task<Sheet> AddTableAsync(int sheetId, AddSheetTableRequest request, CancellationToken cancellationToken)
    {
        return _tables.AddAsync(sheetId, request, cancellationToken);
    }

    public Task<Sheet> SubmitAsync(int sheetId, SubmitSheetRequest request, CancellationToken cancellationToken)
    {
        return _submissionsService.SubmitAsync(sheetId, request, cancellationToken);
    }
}
