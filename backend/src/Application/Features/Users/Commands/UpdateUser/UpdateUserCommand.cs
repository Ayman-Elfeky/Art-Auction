using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Users.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArtAuction.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(Guid Id, string? Name, string? Bio) : IRequest<Result<UserDto>>;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateUserCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user == null) return Result<UserDto>.Failure(new[] { "User not found." });

        if (!string.IsNullOrWhiteSpace(request.Name))
            user.Username = request.Name;

        if (request.Bio != null)
            user.ProfilePictureUrl = request.Bio;

        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<UserDto>.Success(new UserDto(
            user.Id,
            user.Email,
            user.Username,
            user.ProfilePictureUrl,
            user.Role.ToString(),
            user.IsApproved,
            user.CreatedAt));
    }
}
