using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Auth.Commands.Login;
using ArtAuction.Application.Features.Auth.Commands.Register;
using ArtAuction.Application.Features.Auth.DTOs;
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
        try
        {
            var result = await _mediator.Send(new LoginCommand
            {
                Email = input.Email,
                Password = input.Password
            });

            return ToAuthToken(result, "login");
        }
        catch (ArtAuction.Application.Common.Exceptions.ValidationException ex)
        {
            throw new ValidationException(ex.Message, "AUTH_VALIDATION_FAILED");
        }
    }

    public async Task<AuthToken> Register(RegisterInput input)
    {
        try
        {
            var requestedRole = MapPublicRegisterRole(input.Role);
            return await RegisterWithRole(input.Name, input.Email, input.Password, requestedRole);
        }
        catch (ArtAuction.Application.Common.Exceptions.ValidationException ex)
        {
            throw new ValidationException(ex.Message, "AUTH_VALIDATION_FAILED");
        }
    }

    public async Task<AuthToken> RegisterArtist(RegisterArtistInput input)
    {
        try
        {
            return await RegisterWithRole(
                input.Name,
                input.Email,
                input.Password,
                ArtAuction.Domain.Enums.UserRole.Artist);
        }
        catch (ArtAuction.Application.Common.Exceptions.ValidationException ex)
        {
            throw new ValidationException(ex.Message, "AUTH_VALIDATION_FAILED");
        }
    }

    public async Task<User> GetMe()
    {
        var userId = _currentUserContext.GetRequiredUserId();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new UnauthorizedException("User not found.", "AUTH_USER_NOT_FOUND");

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

    private static User MapUser(AuthResponseDto authResponse)
    {
        return new User(
            authResponse.UserId,
            authResponse.Email,
            authResponse.Username,
            null,
            MapRole(authResponse.Role),
            authResponse.IsApproved,
            authResponse.CreatedAt);
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

    private static VeldGenerated.Models.UserRole MapRole(string role)
    {
        return Enum.TryParse<ArtAuction.Domain.Enums.UserRole>(role, true, out var parsedRole)
            ? MapRole(parsedRole)
            : VeldGenerated.Models.UserRole.User;
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

        return ToAuthResponse(result, "register", includeToken: false);
    }

    private static ArtAuction.Domain.Enums.UserRole MapPublicRegisterRole(string? role)
    {
        var normalized = (role ?? string.Empty).Trim().ToLowerInvariant();
        return normalized switch
        {
            "" or "buyer" or "user" => ArtAuction.Domain.Enums.UserRole.Buyer,
            _ => throw new ValidationException("Public registration only supports buyer accounts.", "AUTH_INVALID_ROLE")
        };
    }

    private static AuthToken ToAuthToken(Result<AuthResponseDto> result, string operation)
    {
        return ToAuthResponse(result, operation, includeToken: true);
    }

    private static AuthToken ToAuthResponse(
        Result<AuthResponseDto> result,
        string operation,
        bool includeToken)
    {
        if (!result.Succeeded || result.Data is null)
        {
            throw MapAuthFailure(result.Errors, operation);
        }

        return new AuthToken(includeToken ? result.Data.Token : null, MapUser(result.Data));
    }

    private static ApiException MapAuthFailure(IEnumerable<string> errors, string operation)
    {
        var messages = errors.Where(error => !string.IsNullOrWhiteSpace(error)).ToArray();
        var message = messages.FirstOrDefault() ?? "Authentication failed.";

        return message switch
        {
            "Invalid email or password." => new UnauthorizedException(message, "AUTH_INVALID_CREDENTIALS"),
            "Your account has been deactivated." => new ForbiddenException(message, "AUTH_ACCOUNT_DEACTIVATED"),
            "Email is already registered." => new ConflictException(message, "AUTH_EMAIL_EXISTS"),
            "Username is already taken." => new ConflictException(message, "AUTH_USERNAME_EXISTS"),
            _ when operation == "register" => new BadRequestException(message, "AUTH_REGISTER_FAILED"),
            _ => new UnauthorizedException(message, "AUTH_FAILED")
        };
    }
}
