namespace Workforce.Application.Abstractions.Identity;

public interface ICurrentUserContext
{
    Guid UserId { get; }
    Guid OrganizationId { get; }
    string Role { get; }
}
