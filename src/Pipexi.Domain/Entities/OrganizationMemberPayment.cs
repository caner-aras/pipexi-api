namespace Pipexi.Domain.Entities;

public sealed class OrganizationMemberPayment : BaseEntity
{
    private OrganizationMemberPayment(
        Guid id,
        Guid organizationMemberId,
        decimal amount,
        string currency,
        DateTimeOffset paidAt,
        string method,
        string? reference,
        string? notes,
        DateOnly? periodStart,
        DateOnly? periodEnd,
        string status,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt = null)
        : base(id, status, createdAt)
    {
        OrganizationMemberId = organizationMemberId;
        Amount = amount;
        Currency = currency;
        PaidAt = paidAt;
        Method = method;
        Reference = reference;
        Notes = notes;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        UpdatedAt = updatedAt;
    }

    public Guid OrganizationMemberId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public DateTimeOffset PaidAt { get; private set; }
    public string Method { get; private set; }
    public string? Reference { get; private set; }
    public string? Notes { get; private set; }
    public DateOnly? PeriodStart { get; private set; }
    public DateOnly? PeriodEnd { get; private set; }

    public static OrganizationMemberPayment Create(
        Guid organizationMemberId,
        decimal amount,
        string currency,
        DateTimeOffset paidAt,
        string method,
        string? reference = null,
        string? notes = null,
        DateOnly? periodStart = null,
        DateOnly? periodEnd = null)
    {
        return new OrganizationMemberPayment(
            Guid.NewGuid(),
            organizationMemberId,
            amount,
            currency.Trim().ToUpperInvariant(),
            paidAt,
            method.Trim().ToLowerInvariant(),
            NormalizeOptional(reference),
            NormalizeOptional(notes),
            periodStart,
            periodEnd,
            "active",
            DateTimeOffset.UtcNow);
    }

    public void UpdateDetails(
        decimal? amount,
        string? currency,
        DateTimeOffset? paidAt,
        string? method,
        string? reference,
        string? notes,
        DateOnly? periodStart,
        DateOnly? periodEnd)
    {
        if (amount.HasValue)
        {
            Amount = amount.Value;
        }

        if (currency is not null)
        {
            Currency = currency.Trim().ToUpperInvariant();
        }

        if (paidAt.HasValue)
        {
            PaidAt = paidAt.Value;
        }

        if (method is not null)
        {
            Method = method.Trim().ToLowerInvariant();
        }

        if (reference is not null)
        {
            Reference = NormalizeOptional(reference);
        }

        if (notes is not null)
        {
            Notes = NormalizeOptional(notes);
        }

        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        Touch();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
