using ArtAuction.Application.Features.Auth.Commands.Login;
using ArtAuction.Application.Features.Auth.Commands.Register;
using ArtAuction.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VeldGenerated.Models;
using VeldGenerated.Services;

namespace Api.Services;

public class AuthService : IAuthService
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserContext _currentUserContext;

    public AuthService(IMediator mediator, IApplicationDbContext dbContext, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _dbContext = dbContext;
        _currentUserContext = currentUserContext;
    }

    public async Task<AuthToken> Login(LoginInput input)
    {
        var result = await _mediator.Send(new LoginCommand
        {
            Email = input.Email,
            Password = input.Password
        });

        if (!result.Succeeded || result.Data is null)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors));
        }

        var user = await _dbContext.Users.FirstAsync(u => u.Email == input.Email);
        return new AuthToken(result.Data.Token, MapUser(user));
    }

    public async Task<AuthToken> Register(RegisterInput input)
    {
        var requestedRole = MapPublicRegisterRole(input.Role);
        return await RegisterWithRole(input.Name, input.Email, input.Password, requestedRole);
    }

    public async Task<AuthToken> RegisterArtist(RegisterArtistInput input)
    {
        return await RegisterWithRole(
            input.Name,
            input.Email,
            input.Password,
            ArtAuction.Domain.Enums.UserRole.Artist);
    }

    public async Task<User> GetMe()
    {
        var userId = _currentUserContext.GetRequiredUserId();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found.");
        return MapUser(user);
    }

    public Task<SuccessMessage> Logout()
    {
        return Task.FromResult(new SuccessMessage(true, "Logged out successfully."));
    }

    private static User MapUser(ArtAuction.Domain.Entities.User domainUser)
    {
        return new User(
            domainUser.Id,
            domainUser.Email,
            domainUser.Username,
            null,
            MapRole(domainUser.Role),
            domainUser.IsApproved,
            domainUser.CreatedAt);
    }

    private static VeldGenerated.Models.UserRole MapRole(ArtAuction.Domain.Enums.UserRole role)
    {
        return role switch
        {
            ArtAuction.Domain.Enums.UserRole.Admin => VeldGenerated.Models.UserRole.Admin,
            ArtAuction.Domain.Enums.UserRole.Buyer => VeldGenerated.Models.UserRole.User,
            ArtAuction.Domain.Enums.UserRole.Artist => VeldGenerated.Models.UserRole.Artist,
            _ => VeldGenerated.Models.UserRole.User
        };
    }

    private async Task<AuthToken> RegisterWithRole(
        string username,
        string email,
        string password,
        ArtAuction.Domain.Enums.UserRole role)
    {
        var result = await _mediator.Send(new RegisterCommand
        {
            Username = username,
            Email = email,
            Password = password,
            Role = role
        });

        if (!result.Succeeded || result.Data is null)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors));
        }

        var user = await _dbContext.Users.FirstAsync(u => u.Email == email);
        return new AuthToken(result.Data.Token, MapUser(user));
    }

    private static ArtAuction.Domain.Enums.UserRole MapPublicRegisterRole(string? role)
    {
        var normalized = (role ?? string.Empty).Trim().ToLowerInvariant();
        return normalized switch
        {
            "" or "buyer" or "user" => ArtAuction.Domain.Enums.UserRole.Buyer,
            _ => throw new InvalidOperationException("Not a valid payload")
        };
    }
}