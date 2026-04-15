namespace ArtAuction.Application.Features.Auth.DTOs;

public class AuthResponseDto
{
    public Guid UserId { get; set; }
    public string? Token { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
}
