namespace PUSpecSheet.Domain.Templates;

/// <summary>Which way a table template's sections run across the sheet.</summary>
public enum TemplateOrientation
{
    /// <summary>Sections sit side by side, left to right.</summary>
    Horizontal,

    /// <summary>Sections stack top to bottom.</summary>
    Vertical,
}
