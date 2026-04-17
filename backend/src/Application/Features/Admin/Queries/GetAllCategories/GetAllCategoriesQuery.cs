using ArtAuction.Application.Features.Admin.DTOs;
using MediatR;

namespace ArtAuction.Application.Features.Admin.Queries.GetAllCategories;

public class GetAllCategoriesQuery : IRequest<List<CategoryDto>>
{
}