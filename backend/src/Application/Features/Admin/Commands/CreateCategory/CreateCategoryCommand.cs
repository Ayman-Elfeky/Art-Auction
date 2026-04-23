using ArtAuction.Application.Features.Admin.DTOs;
using MediatR;

namespace ArtAuction.Application.Features.Admin.Commands.CreateCategory;

public class CreateCategoryCommand : IRequest<CategoryDto>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}