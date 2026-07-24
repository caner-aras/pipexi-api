namespace Pipexi.Domain.Entities;

public sealed class FormField : BaseEntity
{
    private FormField(
        Guid id,
        Guid formTemplateId,
        string type,
        string label,
        bool isRequired,
        int sortOrder,
        string? optionsJson,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        FormTemplateId = formTemplateId;
        Type = type;
        Label = label;
        IsRequired = isRequired;
        SortOrder = sortOrder;
        OptionsJson = optionsJson;
        UpdatedAt = updatedAt;
    }

    public Guid FormTemplateId { get; private set; }
    public string Type { get; private set; }
    public string Label { get; private set; }
    public bool IsRequired { get; private set; }
    public int SortOrder { get; private set; }
    public string? OptionsJson { get; private set; }

    public static FormField Create(
        Guid formTemplateId,
        string type,
        string label,
        bool isRequired,
        int sortOrder,
        string? optionsJson)
    {
        return new FormField(
            Guid.NewGuid(),
            formTemplateId,
            type.Trim().ToLowerInvariant(),
            label.Trim(),
            isRequired,
            sortOrder,
            string.IsNullOrWhiteSpace(optionsJson) ? null : optionsJson.Trim(),
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(string? type, string? label, bool? isRequired, int? sortOrder, string? optionsJson, string? status)
    {
        if (type is not null)
        {
            Type = type.Trim().ToLowerInvariant();
        }

        if (label is not null)
        {
            Label = label.Trim();
        }

        if (isRequired.HasValue)
        {
            IsRequired = isRequired.Value;
        }

        if (sortOrder.HasValue)
        {
            SortOrder = sortOrder.Value;
        }

        if (optionsJson is not null)
        {
            OptionsJson = string.IsNullOrWhiteSpace(optionsJson) ? null : optionsJson.Trim();
        }

        if (status is not null)
        {
            SetStatus(status.Trim().ToLowerInvariant());
        }

        if (type is not null ||
            label is not null ||
            isRequired.HasValue ||
            sortOrder.HasValue ||
            optionsJson is not null ||
            status is not null)
        {
            Touch();
        }
    }
}
