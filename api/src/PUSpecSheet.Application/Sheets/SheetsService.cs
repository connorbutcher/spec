using PUSpecSheet.Application.Approvals;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Phases.Queries;
using PUSpecSheet.Application.Sheets.Drafts;
using PUSpecSheet.Application.Sheets.Editing;
using PUSpecSheet.Application.Sheets.Publishing;
using PUSpecSheet.Application.Sheets.Queries;
using PUSpecSheet.Application.Sheets.Reading;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Approvals;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Application.Sheets;

public sealed class SheetsService : ISheetsService
{
    private readonly PuSpecSheetDbContext db;
    private readonly SheetReader reader;
    private readonly SheetPublisher publisher;
    private readonly DraftSweeper sweeper;
    private readonly SubmissionQueries submissions;
    private readonly ICurrentUser currentUser;
    private readonly ISheetTablesService tables;
    private readonly ISubmissionsService submissionsService;

    public SheetsService(
        PuSpecSheetDbContext db,
        SheetReader reader,
        SheetPublisher publisher,
        DraftSweeper sweeper,
        SubmissionQueries submissions,
        ICurrentUser currentUser,
        ISheetTablesService tables,
        ISubmissionsService submissionsService)
    {
        this.db = db;
        this.reader = reader;
        this.publisher = publisher;
        this.sweeper = sweeper;
        this.submissions = submissions;
        this.currentUser = currentUser;
        this.tables = tables;
        this.submissionsService = submissionsService;
    }

    public async Task<SheetDto> OpenAsync(int phaseId, int sheetTypeId, SheetViewPoint view, CancellationToken cancellationToken)
    {
        var sheetId = await SheetQueries.FindIdAsync(db, phaseId, sheetTypeId, cancellationToken);

        if (sheetId is null)
        {
            sheetId = await CreateSheetAsync(phaseId, sheetTypeId, cancellationToken);
        }

        return await GetAsync(sheetId.Value, view, cancellationToken);
    }

    public async Task<SheetDto> GetAsync(int sheetId, SheetViewPoint view, CancellationToken cancellationToken)
    {
        if (view.ReviewedSubmissionId is { } submissionId)
        {
            await submissions.EnsureCanReviewAsync(sheetId, submissionId, currentUser.UserId, cancellationToken);
        }

        if (view.IsLive)
        {
            await sweeper.SweepAsync(sheetId, cancellationToken);
        }

        return await reader.ReadAsync(sheetId, view, cancellationToken);
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

    public async Task<SheetDto> DiscardDraftsAsync(int sheetId, CancellationToken cancellationToken)
    {
        if (!await SheetQueries.ExistsAsync(db, sheetId, cancellationToken))
        {
            throw new NotFoundException($"Sheet {sheetId} was not found.");
        }

        var me = currentUser.UserId;
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        await db.SheetRowRevisions.DeleteOpenDraftsAsync(sheetId, me, cancellationToken);
        await db.SheetColumnBlockRevisions.DeleteOpenDraftsAsync(sheetId, me, cancellationToken);
        await db.SheetSectionRevisions.DeleteOpenDraftsAsync(sheetId, me, cancellationToken);
        await db.SheetTableRevisions.DeleteOpenDraftsAsync(sheetId, me, cancellationToken);

        // Items that only ever existed as the user's drafts now have no revisions at all, so they go too.
        await SheetQueries.DeleteItemsWithoutRevisionsAsync(db, sheetId, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
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

    private async Task<int> CreateSheetAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        var available = await PhaseQueries.HasSheetTypeAsync(db, phaseId, sheetTypeId, cancellationToken);
        if (!available)
        {
            throw new NotFoundException("That sheet type isn't available to that phase.");
        }

        var sheet = new DbSheet { PhaseId = phaseId, SheetTypeId = sheetTypeId };
        db.Sheets.Add(sheet);
        if (await db.TrySaveChangesAsync(cancellationToken))
        {
            return sheet.Id;
        }

        // Someone opened the same sheet at the same moment and created it first.
        db.Detach(sheet);
        return await SheetQueries.FindIdAsync(db, phaseId, sheetTypeId, cancellationToken)
            ?? throw new InvalidOperationException("The sheet could not be created, and nobody else created it either.");
    }
}
