using ArtAuction.Application.Features.Admin.DTOs;
using MediatR;

namespace ArtAuction.Application.Features.Admin.Queries.GetAllTags;

public class GetAllTagsQuery : IRequest<List<TagDto>>
{
}