namespace Pipexi.Contracts.V1.Forms;

public sealed record CreateFormFieldRequest(
    Guid FormTemplateId,
    string Type,
    string Label,
    bool IsRequired,
    int SortOrder,
    string? OptionsJson);
