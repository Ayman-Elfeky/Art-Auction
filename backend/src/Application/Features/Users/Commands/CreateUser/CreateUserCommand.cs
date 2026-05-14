using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Users.DTOs;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArtAuction.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(string Email, string Name, string Password) : IRequest<Result<UserDto>>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateUserCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new ArtAuction.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Username = request.Name,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = ArtAuction.Domain.Enums.UserRole.Buyer,
            IsApproved = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<UserDto>.Success(new UserDto(
            user.Id,
            user.Email,
            user.Username,
            null,
            user.Role.ToString(),
            user.IsApproved,
            user.CreatedAt));
    }
}
