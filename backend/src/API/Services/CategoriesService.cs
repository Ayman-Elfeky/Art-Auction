using ArtAuction.Application.Common.Interfaces;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public interface ICategoriesService
{
    Task<List<Category>> ListCategories();
    Task<Category> CreateCategory(CreateCategoryInput input);
}

public class CategoriesService : ICategoriesService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CategoriesService(IApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<Category>> ListCategories()
    {
        return await _dbContext.Categories
            .OrderBy(c => c.Name)
            .Select(c => new Category(c.Id, c.Name, c.Description))
            .ToListAsync();
    }

    public async Task<Category> CreateCategory(CreateCategoryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new InvalidOperationException("Category name is required.");

        var normalizedName = input.Name.Trim();
        var normalizedLookup = normalizedName.ToLowerInvariant();
        var normalizedDescription = input.Description?.Trim();
        var existing = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == normalizedLookup);

        if (existing is not null)
        {
            if (!string.IsNullOrWhiteSpace(normalizedDescription) && existing.Description != normalizedDescription)
            {
                existing.Description = normalizedDescription;
                await _dbContext.SaveChangesAsync();
            }
            return new Category(existing.Id, existing.Name, existing.Description);
        }

        var category = new ArtAuction.Domain.Entities.Category
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Description = normalizedDescription ?? string.Empty
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();
        return new Category(category.Id, category.Name, category.Description);
    }
}
