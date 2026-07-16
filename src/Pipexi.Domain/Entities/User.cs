namespace Workforce.Domain.Entities;

public sealed class User
{
    private User(
        Guid id,
        string authProviderId,
        string email,
        string firstName,
        string lastName,
        string? phone,
        string? avatarUrl)
    {
        Id = id;
        AuthProviderId = authProviderId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        AvatarUrl = avatarUrl;
    }

    public Guid Id { get; private set; }
    public string AuthProviderId { get; private set; }
    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? Phone { get; private set; }
    public string? AvatarUrl { get; private set; }

    public static User Create(
        Guid id,
        string authProviderId,
        string email,
        string firstName,
        string lastName,
        string? phone,
        string? avatarUrl)
    {
        return new User(
            id,
            authProviderId.Trim(),
            email.Trim().ToLowerInvariant(),
            firstName.Trim(),
            lastName.Trim(),
            string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim());
    }

    public void UpdateProfile(string? firstName, string? lastName, string? phone, string? avatarUrl)
    {
        if (firstName is not null)
        {
            FirstName = firstName.Trim();
        }

        if (lastName is not null)
        {
            LastName = lastName.Trim();
        }

        if (phone is not null)
        {
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        }

        if (avatarUrl is not null)
        {
            AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
        }
    }
}
