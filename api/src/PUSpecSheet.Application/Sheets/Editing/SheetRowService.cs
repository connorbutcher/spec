using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Persistence.Templates;
using PUSpecSheet.Application.Sheets.Collaboration;
using PUSpecSheet.Application.Sheets.Drafts;
using PUSpecSheet.Application.Sheets.Layout;
using PUSpecSheet.Application.Sheets.Linking;
using PUSpecSheet.Application.Sheets.Reading;
using PUSpecSheet.Application.Sheets.Removal;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Application.Sheets.Values;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Sheets.Editing;

public sealed class SheetRowService : ISheetRowService
{
    private readonly ISheetSectionRepository sections;
    private readonly ISheetRowRepository rows;
    private readonly ISheetRowRevisionRepository rowRevisions;
    private readonly ITemplateRowRepository templateRows;
    private readonly IRowValueRepository values;
    private readonly IUnitOfWork unitOfWork;
    private readonly RowDrafts drafts;
    private readonly LiveRowCheckoutGuard liveCheckouts;
    private readonly RowDraftStarter draftStarter;
    private readonly LinkedDropdownValueRule linkedValues;
    private readonly NewRowSettings newRowSettings;
    private readonly SheetInstantiator instantiator;
    private readonly ISheetCellFiller filler;
    private readonly SheetReader reader;
    private readonly IRemovalInProgressGuard removals;
    private readonly ICurrentUser currentUser;

    public SheetRowService(
        ISheetSectionRepository sections,
        ISheetRowRepository rows,
        ISheetRowRevisionRepository rowRevisions,
        ITemplateRowRepository templateRows,
        IRowValueRepository values,
        IUnitOfWork unitOfWork,
        RowDrafts drafts,
        LiveRowCheckoutGuard liveCheckouts,
        RowDraftStarter draftStarter,
        LinkedDropdownValueRule linkedValues,
        NewRowSettings newRowSettings,
        SheetInstantiator instantiator,
        ISheetCellFiller filler,
        SheetReader reader,
        IRemovalInProgressGuard removals,
        ICurrentUser currentUser)
    {
        this.sections = sections;
        this.rows = rows;
        this.rowRevisions = rowRevisions;
        this.templateRows = templateRows;
        this.values = values;
        this.unitOfWork = unitOfWork;
        this.drafts = drafts;
        this.liveCheckouts = liveCheckouts;
        this.draftStarter = draftStarter;
        this.linkedValues = linkedValues;
        this.newRowSettings = newRowSettings;
        this.instantiator = instantiator;
        this.filler = filler;
        this.reader = reader;
        this.removals = removals;
        this.currentUser = currentUser;
    }

    public async Task<SheetDto> AddAsync(int sectionId, AddSheetRowRequest request, CancellationToken cancellationToken)
    {
        var section = await sections.FindWithTemplateAndTableAsync(sectionId, cancellationToken)
            ?? throw new NotFoundException($"Section {sectionId} was not found.");
        if (section.TemplateSection.Role == SectionRole.Header)
        {
            throw new InvalidRequestException("Rows can't be added to the header.");
        }

        var templateRow = await templateRows.FindInSectionWithCellsAsync(request.TemplateRowId, section.TemplateSectionId, cancellationToken)
            ?? throw new InvalidRequestException("That row isn't part of this section's template.");
        await removals.EnsurePlaceNotBeingRemovedAsync(section.SheetTableId, sectionId, currentUser.UserId, cancellationToken);

        var orders = await VisibleRowOrdersAsync(sectionId, excludingRowId: null, cancellationToken);
        var row = instantiator.NewRow(templateRow, section.SheetTable.SheetId, OrderGaps.Next(orders));
        row.SheetSectionId = sectionId;

        rows.Add(row);
        await unitOfWork.SaveSheetChangesAsync(cancellationToken);
        await filler.FillAsync(section.SheetTableId, cancellationToken);
        await newRowSettings.InheritAsync(section.SheetTableId, [row.Id], cancellationToken);
        return await reader.ReadLiveAsync(section.SheetTable.SheetId, cancellationToken);
    }

    public async Task<SheetDto> LockAsync(int rowId, CancellationToken cancellationToken)
    {
        var sheetId = await SheetIdOfAsync(rowId, cancellationToken);

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        await draftStarter.StartAsync(rowId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public async Task<SheetDto> SaveValuesAsync(int rowId, SaveRowValuesRequest request, CancellationToken cancellationToken)
    {
        var row = await LoadRowWithCellTypesAsync(rowId, cancellationToken);

        // Check every value before locking the row or changing anything.
        var cellsById = row.Cells.ToDictionary(cell => cell.Id);
        var changes = ReadValues(request, cellsById);
        await linkedValues.EnsureChoicesAsync(row.SheetSection.SheetTable.SheetId, cellsById, changes, cancellationToken);

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var draft = await draftStarter.StartAsync(rowId, cancellationToken);
        foreach (var change in changes)
        {
            change.Value.Text = change.Value.Text?.Trim();
            change.Value.Slots = SlotValues.Clean(change.Value.Slots);
        }

        await values.SetManyAsync(draft.Id, changes, cancellationToken);
        await unitOfWork.SaveSheetChangesAsync(cancellationToken);

        // Values put back to what is published leave nothing to publish, so the row is released.
        await drafts.ReleaseIfUnchangedAsync(rowId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await reader.ReadLiveAsync(row.SheetSection.SheetTable.SheetId, cancellationToken);
    }

    public async Task<SheetDto> MoveAsync(int rowId, MoveRequest request, CancellationToken cancellationToken)
    {
        var sectionId = await SectionIdOfAsync(rowId, cancellationToken);
        var sheetId = await SheetIdOfAsync(rowId, cancellationToken);

        var siblingOrders = await VisibleRowOrdersAsync(sectionId, rowId, cancellationToken);
        var published = (await drafts.LoadAsync(rowId, cancellationToken)).Current?.DisplayOrder;
        var order = OrderGaps.PlaceAt(siblingOrders, request.DisplayOrder, published);

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var draft = await draftStarter.StartAsync(rowId, cancellationToken);
        draft.DisplayOrder = order;
        await unitOfWork.SaveSheetChangesAsync(cancellationToken);
        await drafts.ReleaseIfUnchangedAsync(rowId, cancellationToken);
        await drafts.ReleaseRestoredOrderAsync(
            await rows.ListIdsInSectionAsync(sectionId, cancellationToken),
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public async Task<SheetDto> RemoveAsync(int rowId, CancellationToken cancellationToken)
    {
        var row = await rows.FindWithSectionAsync(rowId, cancellationToken)
            ?? throw new NotFoundException($"Row {rowId} was not found.");
        if (row.SheetSection.TemplateSection.Role == SectionRole.Header)
        {
            throw new InvalidRequestException("Rows can't be removed from the header.");
        }

        var sheetId = row.SheetSection.SheetTable.SheetId;
        liveCheckouts.EnsureNotHeldByOthers(rowId);
        var state = await drafts.LoadAsync(rowId, cancellationToken);
        await drafts.EnsureNotLockedByOthersAsync(state, cancellationToken);
        await removals.EnsurePlaceNotBeingRemovedAsync(row.SheetSection.SheetTableId, row.SheetSectionId, currentUser.UserId, cancellationToken);

        if (state.IsNew)
        {
            // Only its author ever saw it, so there's no history to keep.
            await rows.DeleteAsync(rowId, cancellationToken);
        }
        else
        {
            var (draft, _) = await drafts.EnsureMineAsync(rowId, state, cancellationToken);
            draft.IsDeleted = true;
            await unitOfWork.SaveSheetChangesAsync(cancellationToken);
        }

        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public async Task<SheetDto> DiscardAsync(int rowId, CancellationToken cancellationToken)
    {
        var sheetId = await SheetIdOfAsync(rowId, cancellationToken);
        var state = await drafts.LoadAsync(rowId, cancellationToken);
        if (state.Draft is { } draft)
        {
            await drafts.EnsureNotLockedByOthersAsync(state, cancellationToken);
            if (state.IsNew)
            {
                await rows.DeleteAsync(rowId, cancellationToken);
            }
            else
            {
                rowRevisions.Remove(draft);
                await unitOfWork.SaveSheetChangesAsync(cancellationToken);
            }
        }

        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    private async Task<int> SheetIdOfAsync(int rowId, CancellationToken cancellationToken)
    {
        return await rows.FindSheetIdAsync(rowId, cancellationToken)
            ?? throw new NotFoundException($"Row {rowId} was not found.");
    }

    private async Task<int> SectionIdOfAsync(int rowId, CancellationToken cancellationToken)
    {
        return await rows.FindSectionIdAsync(rowId, cancellationToken)
            ?? throw new NotFoundException($"Row {rowId} was not found.");
    }

    /// <summary>The display orders of the section's rows the user can see, ascending, optionally leaving one out.</summary>
    private async Task<IReadOnlyList<int>> VisibleRowOrdersAsync(int sectionId, int? excludingRowId, CancellationToken cancellationToken)
    {
        var me = currentUser.UserId;
        var revisions = await rowRevisions.ListVisibleInSectionAsync(sectionId, me, cancellationToken);

        return RevisionResolver.Resolve(revisions, revision => revision.SheetRowId, me)
            .Where(entry => entry.Key != excludingRowId && RevisionResolver.IsVisible(entry.Value))
            .Select(entry => entry.Value.Shown!.DisplayOrder)
            .Order()
            .ToList();
    }

    /// <summary>Reads each value sent in the shape its cell's kind takes, and checks it against the cell.</summary>
    private static List<CellValueChange> ReadValues(SaveRowValuesRequest request, Dictionary<int, SheetCell> cellsById)
    {
        var changes = new List<CellValueChange>();
        foreach (var value in request.Values)
        {
            if (!cellsById.TryGetValue(value.SheetCellId, out var cell))
            {
                throw new InvalidRequestException($"Cell {value.SheetCellId} isn't in this row.");
            }

            var template = cell.TemplateCell;
            var name = string.IsNullOrWhiteSpace(template.Caption) ? template.CellType.Name : template.Caption;
            var bag = CellValueMapper.FromRequest(template.CellType.Kind, value.Value, name);
            CellValueValidator.Validate(template, bag);
            changes.Add(new CellValueChange(value.SheetCellId, template.CellType.Kind, bag));
        }

        return changes;
    }

    private async Task<SheetRow> LoadRowWithCellTypesAsync(int rowId, CancellationToken cancellationToken)
    {
        return await rows.FindWithCellTypesAsync(rowId, withOptions: true, cancellationToken)
            ?? throw new NotFoundException($"Row {rowId} was not found.");
    }
}
