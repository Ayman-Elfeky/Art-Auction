using ArtAuction.Application.Common.Models;
using MediatR;
using System;

namespace ArtAuction.Application.Features.Notifications.Commands.MarkRead;

public record MarkNotificationReadCommand(Guid NotificationId, Guid UserId) : IRequest<Result<string>>;
