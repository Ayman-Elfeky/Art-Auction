namespace Api.Models;

public sealed record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    DateTime CreatedAt,
    Guid? RelatedArtworkId);
