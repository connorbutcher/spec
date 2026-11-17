namespace PUSpecSheet.Abstractions.Approvals;

/// <summary>How many tables, sections, rows and column blocks a submission changes.</summary>
public sealed record SubmissionChangeCountsDto
{
    public required int Tables { get; init; }

    public required int Sections { get; init; }

    public required int Rows { get; init; }

    public required int ColumnBlocks { get; init; }

    public int Total => Tables + Sections + Rows + ColumnBlocks;
}
