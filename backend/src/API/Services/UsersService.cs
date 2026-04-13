using VeldGenerated.Models;
using VeldGenerated.Services;

namespace Api.Services;

public class UsersService : IUsersService
{
    public Task<List<User>> ListUsers(Dictionary<string, string> query)
    {
        IEnumerable<User> users = InMemoryUserStore.Users;

        if (query.TryGetValue("email", out var email) && !string.IsNullOrWhiteSpace(email))
        {
            users = users.Where(u => u.Email.Contains(email, StringComparison.OrdinalIgnoreCase));
        }

        if (query.TryGetValue("name", out var name) && !string.IsNullOrWhiteSpace(name))
        {
            users = users.Where(u => u.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult(users.ToList());
    }

    public Task<User> GetUser(string Id)
    {
        var user = FindUser(Id) ?? InMemoryUserStore.Users.First();
        return Task.FromResult(user);
    }

    public Task<User> CreateUser(CreateUserInput input)
    {
        var user = InMemoryUserStore.Create(input.Email, input.Name, UserRole.User);
        return Task.FromResult(user);
    }

    public Task<User> UpdateUser(string Id, UpdateUserInput input)
    {
        var user = FindUser(Id) ?? InMemoryUserStore.Users.First();

        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            user.Name = input.Name;
        }

        if (input.Bio is not null)
        {
            user.Bio = input.Bio;
        }

        if (input.Role.HasValue)
        {
            user.Role = input.Role.Value;
        }

        return Task.FromResult(user);
    }

    public Task<SuccessMessage> DeleteUser(string Id)
    {
        var user = FindUser(Id);
        var removed = user is not null && InMemoryUserStore.Users.Remove(user);
        return Task.FromResult(new SuccessMessage(removed, removed ? "User deleted." : "User not found."));
    }

    private static User? FindUser(string id)
    {
        return Guid.TryParse(id, out var guid)
            ? InMemoryUserStore.Users.FirstOrDefault(u => u.Id == guid)
            : null;
    }
}
