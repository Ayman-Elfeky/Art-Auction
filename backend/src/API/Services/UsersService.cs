using ArtAuction.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using VeldGenerated.Models;
using VeldGenerated.Services;

namespace Api.Services;

public class UsersService : IUsersService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public UsersService(IApplicationDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public Task<List<User>> ListUsers(Dictionary<string, string> query)
    {
        var usersQuery = _dbContext.Users
            .AsNoTracking()
            .AsQueryable();

        if (query.TryGetValue("email", out var email) && !string.IsNullOrWhiteSpace(email))
        {
            usersQuery = usersQuery.Where(u => u.Email.Contains(email));
        }

        if (query.TryGetValue("name", out var name) && !string.IsNullOrWhiteSpace(name))
        {
            usersQuery = usersQuery.Where(u => u.Username.Contains(name));
        }

        return usersQuery
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new User(
                u.Id,
                u.Email,
                u.Username,
                null,
                u.Role == ArtAuction.Domain.Enums.UserRole.Admin ? UserRole.Admin :
                (u.Role == ArtAuction.Domain.Enums.UserRole.Artist ? UserRole.Artist : UserRole.User),
                u.IsApproved,
                u.CreatedAt))
            .ToListAsync();
    }

    public async Task<User> GetUser(string Id)
    {
        if (!Guid.TryParse(Id, out var guid))
        {
            throw new InvalidOperationException("Invalid user id.");
        }

        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == guid)
            ?? throw new InvalidOperationException("User not found.");
        return new User(
            user.Id,
            user.Email,
            user.Username,
            null,
            user.Role == ArtAuction.Domain.Enums.UserRole.Admin ? UserRole.Admin :
            (user.Role == ArtAuction.Domain.Enums.UserRole.Artist ? UserRole.Artist : UserRole.User),
            user.IsApproved,
            user.CreatedAt);
    }

    public async Task<User> CreateUser(CreateUserInput input)
    {
        var user = new ArtAuction.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = input.Email,
            Username = input.Name,
            PasswordSalt = _passwordHasher.GenerateSalt(),
            PasswordHash = string.Empty,
            Role = ArtAuction.Domain.Enums.UserRole.Buyer,
            IsApproved = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(input.Password, user.PasswordSalt);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return new User(user.Id, user.Email, user.Username, null, UserRole.User, user.IsApproved, user.CreatedAt);
    }

    public async Task<User> UpdateUser(string Id, UpdateUserInput input)
    {
        if (!Guid.TryParse(Id, out var guid))
        {
            throw new InvalidOperationException("Invalid user id.");
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == guid)
            ?? throw new InvalidOperationException("User not found.");

        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            user.Username = input.Name;
        }

        if (input.Bio is not null)
        {
            user.ProfilePictureUrl = input.Bio;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return new User(
            user.Id,
            user.Email,
            user.Username,
            user.ProfilePictureUrl,
            user.Role == ArtAuction.Domain.Enums.UserRole.Admin ? UserRole.Admin :
            (user.Role == ArtAuction.Domain.Enums.UserRole.Artist ? UserRole.Artist : UserRole.User),
            user.IsApproved,
            user.CreatedAt);
    }

    public async Task<SuccessMessage> DeleteUser(string Id)
    {
        if (!Guid.TryParse(Id, out var guid))
        {
            throw new InvalidOperationException("Invalid user id.");
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == guid);
        if (user is null)
        {
            return new SuccessMessage(false, "User not found.");
        }
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
        return new SuccessMessage(true, "User deleted.");
    }
}
