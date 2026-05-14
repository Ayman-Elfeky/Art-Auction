using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Features.Users.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArtAuction.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery(string? Email, string? Name) : IRequest<List<UserDto>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetUsersQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Email))
            query = query.Where(u => u.Email.Contains(request.Email));

        if (!string.IsNullOrWhiteSpace(request.Name))
            query = query.Where(u => u.Username.Contains(request.Name));

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserDto(
                u.Id,
                u.Email,
                u.Username,
                u.ProfilePictureUrl,
                u.Role.ToString(),
                u.IsApproved,
                u.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
