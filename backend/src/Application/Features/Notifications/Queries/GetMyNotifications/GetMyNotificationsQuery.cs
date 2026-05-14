using ArtAuction.Application.Features.Notifications.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace ArtAuction.Application.Features.Notifications.Queries.GetMyNotifications;

public record GetMyNotificationsQuery(Guid UserId) : IRequest<List<NotificationDto>>;
