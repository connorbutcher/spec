using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;

/// <summary>
/// Sets a sheet's property values in its drafts. A property whose value is unchanged is left alone.
/// </summary>
internal class SheetPropertyDrafts
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly User _user;
    private readonly int _sheetId;

    public SheetPropertyDrafts(PUSpecSheetDbContext dbContext, User user, int sheetId)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        _dbContext = dbContext;
        _user = user;
        _sheetId = sheetId;
    }

    /// <summary>
    /// Makes the sheet's property values those given, by type property: a property not given is removed in the draft, as
    /// a new copy held only the values it was created with.
    /// </summary>
    public async Task SetAsync(IReadOnlyDictionary<int, string?> values, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(values, nameof(values));

        var properties = await _dbContext.SheetProperties
            .Where(p => p.SheetId == _sheetId)
            .Include(p => p.Revisions.Where(r => r.Status == RevisionStatus.Draft || r.SupersededDateTime == null))
            .ToListAsync(ct);

        foreach (var (typePropertyId, value) in values)
        {
            var property = properties.FirstOrDefault(p => p.SpecSheetTypePropertyId == typePropertyId);
            if (property is null)
            {
                await AddAsync(typePropertyId, value, ct);
            }
            else
            {
                Set(property, value, false);
            }
        }

        foreach (var property in properties.Where(p => !values.ContainsKey(p.SpecSheetTypePropertyId)))
        {
            Set(property, null, true);
        }
    }

    private async Task AddAsync(int typePropertyId, string? value, CancellationToken ct)
    {
        var property = new DbSheetProperty
        {
            Id = 0,
            SheetId = _sheetId,
            SpecSheetTypePropertyId = typePropertyId,
            CreatedDateTime = DateTime.UtcNow
        };

        property.Revisions.Add(NewDraft(0, 1, value, false));
        await _dbContext.SheetProperties.AddAsync(property, ct);
    }

    private void Set(DbSheetProperty property, string? value, bool isDeleted)
    {
        var draft = property.Revisions.FirstOrDefault(r => r.Status == RevisionStatus.Draft);
        if (draft is not null)
        {
            draft.PropertyValue = isDeleted ? draft.PropertyValue : value;
            draft.IsDeleted = isDeleted;
            draft.LastModifiedDateTime = DateTime.UtcNow;
            return;
        }

        var published = property.Revisions.FirstOrDefault();
        if (published is not null && published.IsDeleted == isDeleted && (isDeleted || published.PropertyValue == value))
        {
            return;
        }

        var started = NewDraft(property.Id, (published?.RevisionNumber ?? 0) + 1, isDeleted ? published?.PropertyValue : value, isDeleted);
        _dbContext.SheetPropertyRevisions.Add(started);
    }

    private DbSheetPropertyRevision NewDraft(int propertyId, int revisionNumber, string? value, bool isDeleted)
    {
        var now = DateTime.UtcNow;
        return new DbSheetPropertyRevision
        {
            Id = 0,
            SheetPropertyId = propertyId,
            SheetId = _sheetId,
            RevisionNumber = revisionNumber,
            Status = RevisionStatus.Draft,
            PropertyValue = value,
            IsDeleted = isDeleted,
            CreatedByUserId = _user.Id,
            CreatedDateTime = now,
            LastModifiedDateTime = now,
            PublishedDateTime = null,
            SupersededDateTime = null,
            SheetVersionId = null,
            SheetSubmissionId = null
        };
    }
}