using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Auth.DTOs;
using MediatR;

namespace ArtAuction.Application.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<Result<AuthResponseDto>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}