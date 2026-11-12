using PUSpecSheet.Application.Approvals;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.Phases;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Sheets.Drafts;
using PUSpecSheet.Application.Sheets.Publishing;
using PUSpecSheet.Application.Sheets.Reading;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

public sealed class SheetService : ISheetService
{
    private readonly ISheetRepository sheets;
    private readonly SheetRevisionRepositories revisions;
    private readonly IPhaseRepository phases;
    private readonly IUnitOfWork unitOfWork;
    private readonly SheetReader reader;
    private readonly SheetPublisher publisher;
    private readonly DraftSweeper sweeper;
    private readonly SubmissionQueries submissions;
    private readonly ICurrentUser currentUser;

    public SheetService(
        ISheetRepository sheets,
        SheetRevisionRepositories revisions,
        IPhaseRepository phases,
        IUnitOfWork unitOfWork,
        SheetReader reader,
        SheetPublisher publisher,
        DraftSweeper sweeper,
        SubmissionQueries submissions,
        ICurrentUser currentUser)
    {
        this.sheets = sheets;
        this.revisions = revisions;
        this.phases = phases;
        this.unitOfWork = unitOfWork;
        this.reader = reader;
        this.publisher = publisher;
        this.sweeper = sweeper;
        this.submissions = submissions;
        this.currentUser = currentUser;
    }

    public async Task<SheetDto> OpenAsync(int phaseId, int sheetTypeId, SheetViewPoint view, CancellationToken cancellationToken)
    {
        var sheetId = await sheets.FindIdAsync(phaseId, sheetTypeId, cancellationToken);

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

    public async Task<SheetDto> PublishAsync(int sheetId, PublishSheetRequest request, CancellationToken cancellationToken)
    {
        await publisher.PublishAsync(sheetId, request.Note, request.Scope, cancellationToken);
        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public async Task<SheetDto> DiscardDraftsAsync(int sheetId, CancellationToken cancellationToken)
    {
        if (!await sheets.ExistsAsync(sheetId, cancellationToken))
        {
            throw new NotFoundException($"Sheet {sheetId} was not found.");
        }

        var me = currentUser.UserId;
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        await revisions.Rows.DeleteOpenDraftsAsync(sheetId, me, cancellationToken);
        await revisions.ColumnBlocks.DeleteOpenDraftsAsync(sheetId, me, cancellationToken);
        await revisions.Sections.DeleteOpenDraftsAsync(sheetId, me, cancellationToken);
        await revisions.Tables.DeleteOpenDraftsAsync(sheetId, me, cancellationToken);

        // Items that only ever existed as the user's drafts now have no revisions at all, so they go too.
        await sheets.DeleteItemsWithoutRevisionsAsync(sheetId, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    private async Task<int> CreateSheetAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        var available = await phases.HasSheetTypeAsync(phaseId, sheetTypeId, cancellationToken);
        if (!available)
        {
            throw new NotFoundException("That sheet type isn't available to that phase.");
        }

        var sheet = new Sheet { PhaseId = phaseId, SheetTypeId = sheetTypeId };
        sheets.Add(sheet);
        if (await unitOfWork.TrySaveChangesAsync(cancellationToken))
        {
            return sheet.Id;
        }

        // Someone opened the same sheet at the same moment and created it first.
        unitOfWork.Detach(sheet);
        return await sheets.FindIdAsync(phaseId, sheetTypeId, cancellationToken)
            ?? throw new InvalidOperationException("The sheet could not be created, and nobody else created it either.");
    }
}
