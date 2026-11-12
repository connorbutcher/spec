namespace PUSpecSheet.Contracts.Approvals;

/// <summary>How many tables, sections, rows and column blocks a submission changes.</summary>
public sealed record SubmissionChangeCountsDto
{
    public SubmissionChangeCountsDto(int tables, int sections, int rows, int columnBlocks)
    {
        this.Tables = tables;
        this.Sections = sections;
        this.Rows = rows;
        this.ColumnBlocks = columnBlocks;
    }

    public int Tables { get; init; }

    public int Sections { get; init; }

    public int Rows { get; init; }

    public int ColumnBlocks { get; init; }

    public int Total => Tables + Sections + Rows + ColumnBlocks;
}
