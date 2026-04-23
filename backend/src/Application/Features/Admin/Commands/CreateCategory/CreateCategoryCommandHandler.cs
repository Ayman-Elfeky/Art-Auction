using ArtAuction.Application.Common.Interfaces;
using ArtAuction.Application.Features.Admin.DTOs;
using ArtAuction.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArtAuction.Application.Features.Admin.Commands.CreateCategory;

public class CreateCategoryCommandHandler
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        // Check if category with same name already exists
        var exists = await _context.Categories
            .AnyAsync(c => c.Name.ToLower() == request.Name.ToLower(), cancellationToken);

        if (exists)
            throw new InvalidOperationException($"Category '{request.Name}' already exists.");

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        // Return the created category as a DTO
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }
}