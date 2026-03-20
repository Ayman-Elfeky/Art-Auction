using ArtAuction.Domain.Enums;

namespace ArtAuction.Application.Features.Auth.DTOs;

public class RegisterRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } // Artist or Buyer only
}