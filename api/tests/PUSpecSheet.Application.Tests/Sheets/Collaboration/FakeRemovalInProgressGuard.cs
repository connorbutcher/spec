using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets.Removal;

namespace PUSpecSheet.Application.Tests.Sheets.Collaboration;

/// <summary>Rows whose section or table someone is removing, standing in for the removal drafts in the database.</summary>
internal sealed class FakeRemovalInProgressGuard : IRemovalInProgressGuard
{
    private readonly Dictionary<int, (int RemoverUserId, string RemoverName)> removedRows = [];

    /// <summary>Someone starts removing the section row <paramref name="rowId"/> is in.</summary>
    public void RemoveSectionOf(int rowId, int removerUserId, string removerName)
    {
        removedRows[rowId] = (removerUserId, removerName);
    }

    public Task EnsureRowNotBeingRemovedAsync(int rowId, int userId, CancellationToken cancellationToken)
    {
        if (removedRows.TryGetValue(rowId, out var removal))
        {
            var refusal = PendingRemovals.Blocking([new PendingRemoval(RemovedContainer.Section, removal.RemoverUserId, AwaitingApproval: false)], userId);
            if (refusal is not null)
            {
                throw new ConflictException(PendingRemovals.RefusalFor(refusal, removal.RemoverName));
            }
        }

        return Task.CompletedTask;
    }

    public Task EnsurePlaceNotBeingRemovedAsync(int tableId, int? sectionId, int userId, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
