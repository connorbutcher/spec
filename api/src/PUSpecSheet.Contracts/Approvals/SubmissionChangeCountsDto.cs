namespace PUSpecSheet.Contracts.Approvals;

/// <summary>How many tables, sections, rows and column blocks a submission changes.</summary>
public sealed record SubmissionChangeCountsDto(int Tables, int Sections, int Rows, int ColumnBlocks)
{
    public int Total => Tables + Sections + Rows + ColumnBlocks;
}
