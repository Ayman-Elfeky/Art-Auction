using ArtAuction.Application.Common.Models;
using MediatR;
using System;

namespace ArtAuction.Application.Features.Notifications.Commands.MarkAllRead;

public record MarkAllNotificationsReadCommand(Guid UserId) : IRequest<Result<string>>;
