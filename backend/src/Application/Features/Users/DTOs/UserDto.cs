using System;

namespace ArtAuction.Application.Features.Users.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string Username,
    string? ProfilePictureUrl,
    string Role,
    bool IsApproved,
    DateTime CreatedAt
);
