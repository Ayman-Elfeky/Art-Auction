using ArtAuction.Application.Features.Admin.DTOs;
using MediatR;

namespace ArtAuction.Application.Features.Admin.Commands.CreateTag;

public class CreateTagCommand : IRequest<TagDto>
{
    public string Tag { get; set; } = string.Empty;
}