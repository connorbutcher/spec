using PUSpecSheet.Abstractions.Approvals;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Application.Approvals;
using PUSpecSheet.Application.Approvals.Queries;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Phases.Queries;
using PUSpecSheet.Application.Sheets.Catalog;
using PUSpecSheet.Application.Sheets.Drafts;
using PUSpecSheet.Application.Sheets.Editing;
using PUSpecSheet.Application.Sheets.Publishing;
using PUSpecSheet.Application.Sheets.Queries;
using PUSpecSheet.Application.Sheets.Reading;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Sheets;

public sealed class SheetsService : ISheetsService
{
    private readonly PuSpecSheetDbContext db;
    private readonly SheetReader reader;
    private readonly SheetPublisher publisher;
    private readonly DraftSweeper sweeper;
    private readonly SubmissionReader submissionReader;
    private readonly ICurrentUser currentUser;
    private readonly ISheetTablesService tables;
    private readonly ISubmissionsService submissionsService;
    private readonly SheetCreator creator;

    public SheetsService(
        PuSpecSheetDbContext db,
        SheetReader reader,
        SheetPublisher publisher,
        DraftSweeper sweeper,
        SubmissionReader submissionReader,
        ICurrentUser currentUser,
        ISheetTablesService tables,
        ISubmissionsService submissionsService,
        SheetCreator creator)
    {
        this.db = db;
        this.reader = reader;
        this.publisher = publisher;
        this.sweeper = sweeper;
        this.submissionReader = submissionReader;
        this.currentUser = currentUser;
        this.tables = tables;
        this.submissionsService = submissionsService;
        this.creator = creator;
    }

    public async Task<SheetDto> OpenAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        var sheetId = await SheetQueries.FindIdAsync(db, phaseId, sheetTypeId, cancellationToken);

        if (sheetId is null)
        {
            sheetId = await creator.CreateUnidentifiedAsync(phaseId, sheetTypeId, cancellationToken);
        }

        return await GetAsync(sheetId.Value, SheetViewPoint.Live, cancellationToken);
    }

    public async Task<SheetDto> GetAsync(int sheetId, SheetViewPoint view, CancellationToken cancellationToken)
    {
        if (view.ReviewedSubmissionId is { } submissionId)
        {
            await submissionReader.EnsureCanReviewAsync(sheetId, submissionId, currentUser.UserId, cancellationToken);
        }

        if (view.IsLive)
        {
            await sweeper.SweepAsync(sheetId, cancellationToken);
        }

        return await reader.ReadAsync(sheetId, view, cancellationToken);
    }

    public async Task<SheetDto> GetForReviewAsync(int submissionId, CancellationToken cancellationToken)
    {
        var sheetId = await SheetSubmissionQueries.FindSheetIdAsync(db, submissionId, cancellationToken)
            ?? throw new NotFoundException($"Submission {submissionId} was not found.");
        return await GetAsync(sheetId, SheetViewPoint.Reviewing(submissionId), cancellationToken);
    }

    public async Task<IReadOnlyList<SheetSummaryDto>> ListAsync(int phaseId, CancellationToken cancellationToken)
    {
        if (!await PhaseQueries.ExistsAsync(db, phaseId, cancellationToken))
        {
            throw new NotFoundException($"Phase {phaseId} was not found.");
        }

        return await SheetCatalogQueries.ListAsync(db, phaseId, null, cancellationToken);
    }

    public async Task<SheetSummaryDto> CreateAsync(int phaseId, CreateSheetRequest request, CancellationToken cancellationToken)
    {
        var sheetId = await creator.CreateAsync(phaseId, request, cancellationToken);
        var created = await SheetCatalogQueries.ListAsync(db, phaseId, sheetId, cancellationToken);
        return created[0];
    }

    public async Task<SheetRowsChangeDto> GetRowsAsync(int sheetId, IReadOnlyCollection<int> rowIds, CancellationToken cancellationToken)
    {
        var sheet = await reader.ReadLiveAsync(sheetId, cancellationToken);
        return SheetRowsChanges.From(sheet, rowIds);
    }

    public async Task<SheetDto> PublishAsync(int sheetId, string? note, PublishScope scope, CancellationToken cancellationToken)
    {
        await publisher.PublishAsync(sheetId, note, scope, cancellationToken);
        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public Task<SheetDto> AddTableAsync(int sheetId, AddSheetTableRequest request, CancellationToken cancellationToken)
    {
        return tables.AddAsync(sheetId, request, cancellationToken);
    }

    public Task<SheetDto> SubmitAsync(int sheetId, SubmitSheetRequest request, CancellationToken cancellationToken)
    {
        return submissionsService.SubmitAsync(sheetId, request, cancellationToken);
    }
}
