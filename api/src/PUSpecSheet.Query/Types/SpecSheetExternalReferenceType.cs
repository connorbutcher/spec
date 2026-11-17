using System.ComponentModel;

namespace PUSpecSheet.Query.Types;

public enum SpecSheetExternalReferenceType
{
    [Description("table")]
    Table = 1,

    [Description("group")]
    Group = 2,

    [Description("row")]
    Row = 3,

    [Description("cell")]
    Cell = 4
}
