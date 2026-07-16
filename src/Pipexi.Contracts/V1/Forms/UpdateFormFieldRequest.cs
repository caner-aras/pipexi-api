namespace Workforce.Contracts.V1.Forms;

public sealed record UpdateFormFieldRequest(
    string? Type,
    string? Label,
    bool? IsRequired,
    int? SortOrder,
    string? OptionsJson,
    string? Status);
