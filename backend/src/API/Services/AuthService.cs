using VeldGenerated.Models;
using VeldGenerated.Services;

namespace Api.Services;

public class AuthService : IAuthService
{
    public Task<AuthToken> Login(LoginInput input)
    {
        var user = InMemoryUserStore.Users.FirstOrDefault(u =>
            string.Equals(u.Email, input.Email, StringComparison.OrdinalIgnoreCase));

        if (user is null)
        {
            user = InMemoryUserStore.Create(input.Email, input.Email.Split('@')[0], UserRole.User);
        }

        var token = new AuthToken(
            Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            user);

        InMemoryUserStore.CurrentUserId = user.Id;
        return Task.FromResult(token);
    }

    public Task<AuthToken> Register(RegisterInput input)
    {
        var user = InMemoryUserStore.Create(input.Email, input.Name, UserRole.User);
        var token = new AuthToken(
            Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            user);

        InMemoryUserStore.CurrentUserId = user.Id;
        return Task.FromResult(token);
    }

    public Task<User> GetMe()
    {
        if (InMemoryUserStore.CurrentUserId is Guid currentUserId)
        {
            var currentUser = InMemoryUserStore.Users.FirstOrDefault(u => u.Id == currentUserId);
            if (currentUser is not null)
            {
                return Task.FromResult(currentUser);
            }
        }

        return Task.FromResult(InMemoryUserStore.Users.First());
    }

    public Task<SuccessMessage> Logout()
    {
        InMemoryUserStore.CurrentUserId = null;
        return Task.FromResult(new SuccessMessage(true, "Logged out successfully."));
    }
}