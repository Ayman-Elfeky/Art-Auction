using VeldGenerated.Models;

namespace Api.Services;

internal static class InMemoryUserStore
{
    private static readonly List<User> _users =
    [
        new User(
            Guid.NewGuid(),
            "admin@example.com",
            "Admin",
            "Seeded admin user",
            UserRole.Admin,
            true,
            DateTime.UtcNow)
    ];

    public static List<User> Users => _users;

    public static Guid? CurrentUserId { get; set; } = _users[0].Id;

    public static User Create(string email, string name, UserRole role)
    {
        var user = new User(
            Guid.NewGuid(),
            email,
            name,
            null,
            role,
            true,
            DateTime.UtcNow);

        _users.Add(user);
        return user;
    }
}
