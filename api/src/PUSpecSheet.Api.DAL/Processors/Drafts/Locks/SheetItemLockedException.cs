namespace PUSpecSheet.Api.DAL.Processors.Drafts.Locks;

/// <summary>
/// Thrown when an edit touches an item that is checked out to someone else, or that is awaiting approval. An item is
/// checked out to whoever made its draft, until they undo or submit it.
/// </summary>
public class SheetItemLockedException : InvalidOperationException
{
    public SheetItemLockedException(string message)
        : base(message)
    {
    }
}
