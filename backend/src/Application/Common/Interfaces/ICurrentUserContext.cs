using ArtAuction.Domain.Enums;
using System;

namespace ArtAuction.Application.Common.Interfaces;

public interface ICurrentUserContext
{
    Guid GetRequiredUserId();
    bool IsAuthenticated();
    bool IsInRole(UserRole role);
}
