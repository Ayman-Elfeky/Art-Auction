using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Auth.DTOs;
using MediatR;

namespace ArtAuction.Application.Features.Auth.Queries.GetMe;

public record GetMeQuery(Guid UserId) : IRequest<Result<UserDto>>;
