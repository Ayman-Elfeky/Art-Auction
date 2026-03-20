using ArtAuction.Application.Common.Models;
using ArtAuction.Application.Features.Auth.DTOs;
using ArtAuction.Domain.Enums;
using MediatR;

namespace ArtAuction.Application.Features.Auth.Commands.Register;

public class RegisterCommand : IRequest<Result<AuthResponseDto>>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}